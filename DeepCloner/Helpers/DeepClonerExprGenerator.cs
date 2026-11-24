using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace PanoramicData.DeepCloner.Helpers;

internal static partial class DeepClonerExprGenerator
{
	private static readonly ConcurrentDictionary<FieldInfo, bool> _readonlyFields = new ConcurrentDictionary<FieldInfo, bool>();
	private static readonly MethodInfo _addKnownRefMethod = typeof(DeepCloneState).GetMethod(nameof(DeepCloneState.AddKnownRef))!;

	private static bool _canFastCopyReadonlyFields;

	private static readonly MethodInfo? _fieldSetMethod;
	static DeepClonerExprGenerator()
	{
		try
		{
			_canFastCopyReadonlyFields = true;
#if NETCORE13
			_fieldSetMethod = typeof(FieldInfo).GetRuntimeMethod("SetValue", new[] { typeof(object), typeof(object) });
#else
			_fieldSetMethod = typeof(FieldInfo).GetMethod("SetValue", new[] { typeof(object), typeof(object) });
#endif
		}
		catch (Exception)
		{
			// cannot
		}
	}

	internal static object GenerateClonerInternal(Type realType, bool asObject)
	{
		return GenerateProcessMethod(realType, asObject && realType.IsValueType());
	}

   private static readonly FieldInfo? _attributesFieldInfo = typeof(FieldInfo).GetPrivateField("m_fieldAttributes");

	// today, I found that it not required to do such complex things. Just SetValue is enough
	// is it new runtime changes, or I made incorrect assumptions eariler
	// slow, but hardcore method to set readonly field
	internal static void ForceSetField(FieldInfo field, object obj, object value)
	{
		var fieldInfo = field.GetType().GetPrivateField("m_fieldAttributes");

		// TODO: think about it
		// nothing to do :( we should a throw an exception, but it is no good for user
		if (fieldInfo == null)
		{
			return;
		}
		var ov = fieldInfo.GetValue(field);
		if (!(ov is FieldAttributes))
		{
			return;
		}
		var v = (FieldAttributes)ov;

		// protect from parallel execution, when first thread set field readonly back, and second set it to write value
		lock (fieldInfo)
		{
			fieldInfo.SetValue(field, v & ~FieldAttributes.InitOnly);
			field.SetValue(obj, value);
			fieldInfo.SetValue(field, v | FieldAttributes.InitOnly);
		}
	}

	private static object GenerateProcessMethod(Type type, bool unboxStruct)
	{
		if (type.IsArray)
		{
			return GenerateProcessArrayMethod(type);
		}

		if (IsCloneableTuple(type))
		{
			return GenerateProcessTupleMethod(type);
		}

		var methodType = unboxStruct || type.IsClass() ? typeof(object) : type;

		var expressionList = new List<Expression>();

		ParameterExpression from = Expression.Parameter(methodType);
		ParameterExpression fromLocal = from;
		var toLocal = Expression.Variable(type);
		var state = Expression.Parameter(typeof(DeepCloneState));

		if (!type.IsValueType())
		{
			AddClassInitialisation(expressionList, type, from, toLocal, state, ref fromLocal);
		}
		else
		{
			AddStructInitialisation(expressionList, type, unboxStruct, from, toLocal, ref fromLocal);
		}

		foreach (var fieldInfo in GetInheritedFields(type))
		{
			AddFieldCloneExpression(expressionList, fieldInfo, fromLocal, toLocal, state);
		}

		expressionList.Add(Expression.Convert(toLocal, methodType));

		var funcType = typeof(Func<,,>).MakeGenericType(methodType, typeof(DeepCloneState), methodType);

		var blockParams = new List<ParameterExpression>();
		if (from != fromLocal)
		{
			blockParams.Add(fromLocal);
		}
		blockParams.Add(toLocal);

		return Expression.Lambda(funcType, Expression.Block(blockParams, expressionList), from, state).Compile();
	}

	private static bool IsCloneableTuple(Type type)
	{
		if (type.FullName == null || !type.FullName.StartsWith("System.Tuple`", StringComparison.Ordinal))
		{
			return false;
		}

		// if not safe type it is no guarantee that some type will contain reference to
		// this tuple. In usual way, we're creating new object, setting reference for it
		// and filling data. For tuple, we will fill data before creating object
		// (in constructor arguments)
		var genericArguments = type.GenericArguments();
		// current tuples contain only 8 arguments, but may be in future...
		// we'll write code that works with it
		return genericArguments.Length < 10 && genericArguments.All(DeepClonerSafeTypes.CanReturnSameObject);
	}
}
