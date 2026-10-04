#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace PanoramicData.DeepCloner.Helpers;

internal static class ClonerToExprGenerator
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

	// when we can't use code generation, we can use these methods
	internal static T[] ShallowClone1DimArraySafeInternal<T>(T[] objFrom, T[] objTo)
	{
		var l = Math.Min(objFrom.Length, objTo.Length);
		Array.Copy(objFrom, objTo, l);
		return objTo;
	}

	// when we can't use code generation, we can use these methods
	internal static T[] Clone1DimArraySafeInternal<T>(T[] objFrom, T[] objTo, DeepCloneState state)
	{
		var l = Math.Min(objFrom.Length, objTo.Length);
		state.AddKnownRef(objFrom, objTo);
		Array.Copy(objFrom, objTo, l);
		return objTo;
	}

	internal static T[] Clone1DimArrayStructInternal<T>(T[] objFrom, T[] objTo, DeepCloneState state)
	{
		// not null from called method, but will check it anyway
		if (objFrom == null || objTo == null)
		{
			return null;
		}

		var l = Math.Min(objFrom.Length, objTo.Length);
		state.AddKnownRef(objFrom, objTo);
		var cloner = DeepClonerGenerator.GetClonerForValueType<T>();
		for (var i = 0; i < l; i++)
		{
			objTo[i] = cloner(objTo[i], state);
		}

		return objTo;
	}

	internal static T[] Clone1DimArrayClassInternal<T>(T[] objFrom, T[] objTo, DeepCloneState state)
	{
		// not null from called method, but will check it anyway
		if (objFrom == null || objTo == null)
		{
			return null;
		}

		var l = Math.Min(objFrom.Length, objTo.Length);
		state.AddKnownRef(objFrom, objTo);
		for (var i = 0; i < l; i++)
		{
			objTo[i] = (T)DeepClonerGenerator.CloneClassInternal(objFrom[i], state);
		}

		return objTo;
	}

	internal static T[,] Clone2DimArrayInternal<T>(T[,] objFrom, T[,] objTo, DeepCloneState state, bool isDeep)
	{
		// not null from called method, but will check it anyway
		if (objFrom == null || objTo == null)
		{
			return null;
		}

		if (HasNonZeroLowerBound(objFrom) || HasNonZeroLowerBound(objTo))
		{
			return (T[,])CloneAbstractArrayInternal(objFrom, objTo, state, isDeep);
		}

		var l1 = Math.Min(objFrom.GetLength(0), objTo.GetLength(0));
		var l2 = Math.Min(objFrom.GetLength(1), objTo.GetLength(1));
		state.AddKnownRef(objFrom, objTo);
		if ((!isDeep || DeepClonerSafeTypes.CanReturnSameObject(typeof(T))) && HaveSameShape(objFrom, objTo))
		{
			Array.Copy(objFrom, objTo, objFrom.Length);
			return objTo;
		}

		if (!isDeep)
		{
			Copy2DimArray(objFrom, objTo, l1, l2);
		}
		else if (typeof(T).IsValueType())
		{
			Clone2DimArrayStructs(objFrom, objTo, l1, l2, state);
		}
		else
		{
			Clone2DimArrayClasses(objFrom, objTo, l1, l2, state);
		}

		return objTo;
	}

	private static bool HasNonZeroLowerBound(Array array)
		=> array.GetLowerBound(0) != 0 || array.GetLowerBound(1) != 0;

	private static bool HaveSameShape(Array first, Array second)
		=> first.GetLength(0) == second.GetLength(0) && first.GetLength(1) == second.GetLength(1);

	private static void Copy2DimArray<T>(T[,] objFrom, T[,] objTo, int l1, int l2)
	{
		for (var i = 0; i < l1; i++)
		{
			for (var k = 0; k < l2; k++)
			{
				objTo[i, k] = objFrom[i, k];
			}
		}
	}

	private static void Clone2DimArrayStructs<T>(T[,] objFrom, T[,] objTo, int l1, int l2, DeepCloneState state)
	{
		var cloner = DeepClonerGenerator.GetClonerForValueType<T>();
		for (var i = 0; i < l1; i++)
		{
			for (var k = 0; k < l2; k++)
			{
				objTo[i, k] = cloner(objFrom[i, k], state);
			}
		}
	}

	private static void Clone2DimArrayClasses<T>(T[,] objFrom, T[,] objTo, int l1, int l2, DeepCloneState state)
	{
		for (var i = 0; i < l1; i++)
		{
			for (var k = 0; k < l2; k++)
			{
				objTo[i, k] = (T)DeepClonerGenerator.CloneClassInternal(objFrom[i, k], state);
			}
		}
	}

	// rare cases, very slow cloning. currently it's ok
	internal static Array CloneAbstractArrayInternal(Array objFrom, Array objTo, DeepCloneState state, bool isDeep)
	{
		// not null from called method, but will check it anyway
		if (objFrom == null || objTo == null)
		{
			return null;
		}

		var rank = objFrom.Rank;

		if (objTo.Rank != rank)
		{
			throw new InvalidOperationException("Invalid rank of target array");
		}

		var lowerBoundsFrom = Enumerable.Range(0, rank).Select(objFrom.GetLowerBound).ToArray();
		var lowerBoundsTo = Enumerable.Range(0, rank).Select(objTo.GetLowerBound).ToArray();
		var lengths = Enumerable.Range(0, rank).Select(x => Math.Min(objFrom.GetLength(x), objTo.GetLength(x))).ToArray();
		var idxesFrom = Enumerable.Range(0, rank).Select(objFrom.GetLowerBound).ToArray();
		var idxesTo = Enumerable.Range(0, rank).Select(objTo.GetLowerBound).ToArray();

		state.AddKnownRef(objFrom, objTo);

		// unable to copy any element
		if (lengths.Any(x => x == 0))
		{
			return objTo;
		}

		do
		{
			var value = objFrom.GetValue(idxesFrom);
			objTo.SetValue(isDeep ? DeepClonerGenerator.CloneClassInternal(value, state) : value, idxesTo);
		}
		while (AdvanceIndexes(idxesFrom, idxesTo, lowerBoundsFrom, lowerBoundsTo, lengths));

		return objTo;
	}

	/// <summary>
	/// Moves both index vectors to the next element, odometer style. Returns false once every element has been visited.
	/// </summary>
	private static bool AdvanceIndexes(int[] idxesFrom, int[] idxesTo, int[] lowerBoundsFrom, int[] lowerBoundsTo, int[] lengths)
	{
		var ofs = idxesFrom.Length - 1;
		while (ofs >= 0)
		{
			idxesFrom[ofs]++;
			idxesTo[ofs]++;
			if (idxesFrom[ofs] < lowerBoundsFrom[ofs] + lengths[ofs])
			{
				return true;
			}

			idxesFrom[ofs] = lowerBoundsFrom[ofs];
			idxesTo[ofs] = lowerBoundsTo[ofs];
			ofs--;
		}

		return false;
	}
}
