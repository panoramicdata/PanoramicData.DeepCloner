#nullable disable

using System;
using System.Linq;

namespace PanoramicData.DeepCloner.Helpers;

internal static partial class DeepClonerGenerator
{
	public static T CloneObject<T>(T obj)
	{
		if (obj is ValueType)
		{
			var type = obj.GetType();
			if (typeof(T) == type)
			{
				if (DeepClonerSafeTypes.CanReturnSameObject(type))
				{
					return obj;
				}

				return CloneStructInternal(obj, new DeepCloneState());
			}
		}

		return (T)CloneClassRoot(obj);
	}

	private static object CloneClassRoot(object obj)
	{
		if (obj == null)
		{
			return null;
		}

		var cloner = (Func<object, DeepCloneState, object>)DeepClonerCache.GetOrAddClass(obj.GetType(), t => GenerateCloner(t, true));

		// null -> should return same type
		if (cloner == null)
		{
			return obj;
		}

		return cloner(obj, new DeepCloneState());
	}

	internal static object CloneClassInternal(object obj, DeepCloneState state)
	{
		if (obj == null)
		{
			return null;
		}

		var cloner = (Func<object, DeepCloneState, object>)DeepClonerCache.GetOrAddClass(obj.GetType(), t => GenerateCloner(t, true));

		// safe object
		if (cloner == null)
		{
			return obj;
		}

		// loop
		var knownRef = state.GetKnownRef(obj);
		if (knownRef != null)
		{
			return knownRef;
		}

		return cloner(obj, state);
	}

	private static T CloneStructInternal<T>(T obj, DeepCloneState state) // where T : struct
	{
		// no loops, no nulls, no inheritance
		var cloner = GetClonerForValueType<T>();

		// safe ojbect
		if (cloner == null)
		{
			return obj;
		}

		return cloner(obj, state);
	}
}
