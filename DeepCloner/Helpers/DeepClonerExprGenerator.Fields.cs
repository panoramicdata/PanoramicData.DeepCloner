using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace PanoramicData.DeepCloner.Helpers;

internal static partial class DeepClonerExprGenerator
{
	private static void AddClassInitialisation(List<Expression> expressionList, Type type, Expression from, Expression toLocal, Expression state, ref ParameterExpression fromLocal)
	{
        var methodInfo = typeof(object).GetPrivateMethod("MemberwiseClone")!;

		// to = (T)from.MemberwiseClone()
		expressionList.Add(Expression.Assign(toLocal, Expression.Convert(Expression.Call(from, methodInfo), type)));

		fromLocal = Expression.Variable(type);
		// fromLocal = (T)from
		expressionList.Add(Expression.Assign(fromLocal, Expression.Convert(from, type)));

		// added from -> to binding to ensure reference loop handling
		// structs cannot loop here
         expressionList.Add(Expression.Call(state, _addKnownRefMethod, from, toLocal));
	}

	private static void AddStructInitialisation(List<Expression> expressionList, Type type, bool unboxStruct, Expression from, Expression toLocal, ref ParameterExpression fromLocal)
	{
		if (unboxStruct)
		{
			expressionList.Add(Expression.Assign(toLocal, Expression.Unbox(from, type)));
			fromLocal = Expression.Variable(type);
			expressionList.Add(Expression.Assign(fromLocal, toLocal));
		}
		else
		{
			// toLocal = from
			expressionList.Add(Expression.Assign(toLocal, from));
		}
	}

	private static List<FieldInfo> GetInheritedFields(Type type)
	{
		List<FieldInfo> fi = new List<FieldInfo>();
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

			fi.AddRange(tp.GetDeclaredFields());
			tp = tp.BaseType();
		}
		while (tp != null);

		return fi;
	}

	private static void AddFieldCloneExpression(List<Expression> expressionList, FieldInfo fieldInfo, Expression fromLocal, Expression toLocal, Expression state)
	{
		if (DeepClonerSafeTypes.CanReturnSameObject(fieldInfo.FieldType))
		{
			return;
		}

       var methodInfo = fieldInfo.FieldType.IsValueType()
								? typeof(DeepClonerGenerator).GetPrivateStaticMethod("CloneStructInternal")!
										.MakeGenericMethod(fieldInfo.FieldType)
								: typeof(DeepClonerGenerator).GetPrivateStaticMethod("CloneClassInternal")!;

		var get = Expression.Field(fromLocal, fieldInfo);

		// toLocal.Field = Clone...Internal(fromLocal.Field)
		var call = (Expression)Expression.Call(methodInfo, get, state);
		if (!fieldInfo.FieldType.IsValueType())
		{
			call = Expression.Convert(call, fieldInfo.FieldType);
		}

		// should handle specially
		// todo: think about optimization, but it rare case
		var isReadonly = _readonlyFields.GetOrAdd(fieldInfo, f => f.IsInitOnly);
		var fieldConstant = Expression.Constant(fieldInfo);
		if (isReadonly)
		{
			if (_canFastCopyReadonlyFields)
			{
				// fieldInfo.SetValue(toLocal, value): FieldInfo.SetValue is an INSTANCE method, so the
				// FieldInfo constant is the call target, not the first argument.
				expressionList.Add(SetReadonlyField(toLocal, boxed => Expression.Call(
					fieldConstant,
					_fieldSetMethod!,
					boxed,
					Expression.Convert(call, typeof(object)))));
			}
			else
			{
                var setMethod = typeof(DeepClonerExprGenerator).GetPrivateStaticMethod("ForceSetField")!;
				// ForceSetField(fieldInfo, toLocal, value): a STATIC method, so there is no call target.
				expressionList.Add(SetReadonlyField(toLocal, boxed => Expression.Call(
					setMethod,
					fieldConstant,
					boxed,
					Expression.Convert(call, typeof(object)))));
			}
		}
		else
		{
			expressionList.Add(Expression.Assign(Expression.Field(toLocal, fieldInfo), call));
		}
	}

	/// <summary>
	/// Builds the expression that writes a readonly field through reflection, which needs the target as an object.
	/// Boxing a class just passes the reference, but boxing a struct copies it, so the write would land on the copy
	/// and be thrown away. For a struct the target is therefore boxed once, written, and unboxed back.
	/// </summary>
	private static Expression SetReadonlyField(Expression toLocal, Func<Expression, Expression> setField)
	{
		if (!toLocal.Type.IsValueType())
		{
			return setField(Expression.Convert(toLocal, typeof(object)));
		}

		var boxed = Expression.Variable(typeof(object));
		return Expression.Block(
			[boxed],
			Expression.Assign(boxed, Expression.Convert(toLocal, typeof(object))),
			setField(boxed),
			Expression.Assign(toLocal, Expression.Unbox(boxed, toLocal.Type)));
	}
}
