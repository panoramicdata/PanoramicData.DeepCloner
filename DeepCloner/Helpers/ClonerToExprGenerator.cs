#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace PanoramicData.DeepCloner.Helpers;

internal static partial class ClonerToExprGenerator
{
	internal static object GenerateClonerInternal(Type realType, bool isDeepClone)
	{
		if (realType.IsValueType())
		{
			throw new InvalidOperationException("Operation is valid only for reference types");
		}

		return GenerateProcessMethod(realType, isDeepClone);
	}

	private static object GenerateProcessMethod(Type type, bool isDeepClone)
	{
		if (type.IsArray)
		{
			return GenerateProcessArrayMethod(type, isDeepClone);
		}

		var methodType = typeof(object);

		var expressionList = new List<Expression>();

		var from = Expression.Parameter(methodType);
		var to = Expression.Parameter(methodType);
		var state = Expression.Parameter(typeof(DeepCloneState));

		var fromLocal = Expression.Variable(type);
		var toLocal = Expression.Variable(type);

		// fromLocal = (T)from
		expressionList.Add(Expression.Assign(fromLocal, Expression.Convert(from, type)));
		expressionList.Add(Expression.Assign(toLocal, Expression.Convert(to, type)));

		if (isDeepClone)
		{
			// added from -> to binding to ensure reference loop handling
			// structs cannot loop here
			// state.AddKnownRef(from, to)
			expressionList.Add(Expression.Call(state, typeof(DeepCloneState).GetMethod("AddKnownRef"), from, to));
		}

		foreach (var fieldInfo in GetAllFields(type))
		{
			expressionList.Add(GenerateFieldCopy(fieldInfo, fromLocal, toLocal, state, isDeepClone));
		}

		expressionList.Add(Expression.Convert(toLocal, methodType));

		var funcType = typeof(Func<,,,>).MakeGenericType(methodType, methodType, typeof(DeepCloneState), methodType);

		return Expression.Lambda(funcType, Expression.Block(new[] { fromLocal, toLocal }, expressionList), from, to, state).Compile();
	}

	private static List<FieldInfo> GetAllFields(Type type)
	{
		var fields = new List<FieldInfo>();
		var tp = type;
		do
		{
#if !NETCORE
			// don't do anything with this dark magic!
			if (tp == typeof(ContextBoundObject))
			{
				break;
			}
#else
			if (tp.Name == "ContextBoundObject")
			{
				break;
			}
#endif

			fields.AddRange(tp.GetDeclaredFields());
			tp = tp.BaseType();
		}
		while (tp != null);

		return fields;
	}

	private static Expression GenerateFieldCopy(
		FieldInfo fieldInfo,
		ParameterExpression fromLocal,
		ParameterExpression toLocal,
		ParameterExpression state,
		bool isDeepClone)
	{
		if (!isDeepClone || DeepClonerSafeTypes.CanReturnSameObject(fieldInfo.FieldType))
		{
			return Expression.Assign(Expression.Field(toLocal, fieldInfo), Expression.Field(fromLocal, fieldInfo));
		}

		var methodInfo = fieldInfo.FieldType.IsValueType()
			? typeof(DeepClonerGenerator).GetPrivateStaticMethod("CloneStructInternal")
				.MakeGenericMethod(fieldInfo.FieldType)
			: typeof(DeepClonerGenerator).GetPrivateStaticMethod("CloneClassInternal");

		var get = Expression.Field(fromLocal, fieldInfo);

		// toLocal.Field = Clone...Internal(fromLocal.Field)
		var call = (Expression)Expression.Call(methodInfo, get, state);
		if (!fieldInfo.FieldType.IsValueType())
		{
			call = Expression.Convert(call, fieldInfo.FieldType);
		}

		// should handle specially
		// todo: think about optimization, but it rare case
		if (fieldInfo.IsInitOnly)
		{
			var setMethod = typeof(DeepClonerExprGenerator).GetPrivateStaticMethod("ForceSetField");
			return Expression.Call(setMethod, Expression.Constant(fieldInfo),
				Expression.Convert(toLocal, typeof(object)), Expression.Convert(call, typeof(object)));
		}

		return Expression.Assign(Expression.Field(toLocal, fieldInfo), call);
	}

	private static object GenerateProcessArrayMethod(Type type, bool isDeep)
	{
		var elementType = type.GetElementType();
		var rank = type.GetArrayRank();

		ParameterExpression from = Expression.Parameter(typeof(object));
		ParameterExpression to = Expression.Parameter(typeof(object));
		var state = Expression.Parameter(typeof(DeepCloneState));

		var funcType = typeof(Func<,,,>).MakeGenericType(typeof(object), typeof(object), typeof(DeepCloneState), typeof(object));

		if (rank == 1 && type == elementType.MakeArrayType())
		{
			return GenerateProcessOneDimArrayMethod(type, elementType, isDeep, funcType, from, to, state);
		}

		// multidim or not zero-based arrays
		var methodInfo = rank == 2 && type == elementType.MakeArrayType(2)
			? typeof(ClonerToExprGenerator).GetPrivateStaticMethod("Clone2DimArrayInternal").MakeGenericMethod(elementType)
			: typeof(ClonerToExprGenerator).GetPrivateStaticMethod("CloneAbstractArrayInternal");

		var callS = Expression.Call(methodInfo, Expression.Convert(from, type), Expression.Convert(to, type), state, Expression.Constant(isDeep));
		return Expression.Lambda(funcType, callS, from, to, state).Compile();
	}

	private static object GenerateProcessOneDimArrayMethod(
		Type type,
		Type elementType,
		bool isDeep,
		Type funcType,
		ParameterExpression from,
		ParameterExpression to,
		ParameterExpression state)
	{
		if (!isDeep)
		{
			var callS = Expression.Call(
				typeof(ClonerToExprGenerator).GetPrivateStaticMethod("ShallowClone1DimArraySafeInternal")
					.MakeGenericMethod(elementType), Expression.Convert(from, type), Expression.Convert(to, type));
			return Expression.Lambda(funcType, callS, from, to, state).Compile();
		}

		var methodName = "Clone1DimArrayClassInternal";
		if (DeepClonerSafeTypes.CanReturnSameObject(elementType))
		{
			methodName = "Clone1DimArraySafeInternal";
		}
		else if (elementType.IsValueType())
		{
			methodName = "Clone1DimArrayStructInternal";
		}

		var methodInfo = typeof(ClonerToExprGenerator).GetPrivateStaticMethod(methodName).MakeGenericMethod(elementType);
		var callD = Expression.Call(methodInfo, Expression.Convert(from, type), Expression.Convert(to, type), state);
		return Expression.Lambda(funcType, callD, from, to, state).Compile();
	}
}
