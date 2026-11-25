using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace PanoramicData.DeepCloner.Helpers;

internal static partial class DeepClonerExprGenerator
{
	private static object GenerateProcessArrayMethod(Type type)
	{
        var elementType = type.GetElementType()!;
		var rank = type.GetArrayRank();

		MethodInfo methodInfo;

		// multidim or not zero-based arrays
		if (rank != 1 || type != elementType.MakeArrayType())
		{
			if (rank == 2 && type == elementType.MakeArrayType(2))
			{
				// small optimization for 2 dim arrays
               methodInfo = typeof(DeepClonerGenerator).GetPrivateStaticMethod("Clone2DimArrayInternal")!.MakeGenericMethod(elementType);
			}
			else
			{
              methodInfo = typeof(DeepClonerGenerator).GetPrivateStaticMethod("CloneAbstractArrayInternal")!;
			}
		}
		else
		{
			var methodName = "Clone1DimArrayClassInternal";
			if (DeepClonerSafeTypes.CanReturnSameObject(elementType))
			{
				methodName = "Clone1DimArraySafeInternal";
			}
			else if (elementType.IsValueType())
			{
				methodName = "Clone1DimArrayStructInternal";
			}
         methodInfo = typeof(DeepClonerGenerator).GetPrivateStaticMethod(methodName)!.MakeGenericMethod(elementType);
		}

		ParameterExpression from = Expression.Parameter(typeof(object));
		var state = Expression.Parameter(typeof(DeepCloneState));
		var call = Expression.Call(methodInfo, Expression.Convert(from, type), state);

		var funcType = typeof(Func<,,>).MakeGenericType(typeof(object), typeof(DeepCloneState), typeof(object));

		return Expression.Lambda(funcType, call, from, state).Compile();
	}

	private static object GenerateProcessTupleMethod(Type type)
	{
		ParameterExpression from = Expression.Parameter(typeof(object));
		var state = Expression.Parameter(typeof(DeepCloneState));

		var local = Expression.Variable(type);
		var assign = Expression.Assign(local, Expression.Convert(from, type));

		var funcType = typeof(Func<object, DeepCloneState, object>);

		var tupleLength = type.GenericArguments().Length;

		var constructor = Expression.Assign(local, Expression.New(type.GetPublicConstructors().First(x => x.GetParameters().Length == tupleLength),
			type.GetPublicProperties().OrderBy(x => x.Name)
              .Where(x => x.CanRead && x.Name.StartsWith("Item", StringComparison.Ordinal) && char.IsDigit(x.Name[4]))
				.Select(x => Expression.Property(local, x.Name))));

		var register = Expression.Call(state, _addKnownRefMethod, from, local);
		Expression[] body = [assign, constructor, register, from];

		return Expression.Lambda(funcType, Expression.Block([local], body), from, state).Compile();
	}
}
