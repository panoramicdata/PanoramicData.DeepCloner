#nullable disable

using System;
using System.Linq;

namespace PanoramicData.DeepCloner.Helpers;

internal static partial class DeepClonerGenerator
{
	internal static Func<T, DeepCloneState, T> GetClonerForValueType<T>()
	{
		return (Func<T, DeepCloneState, T>)DeepClonerCache.GetOrAddStructAsObject(typeof(T), t => GenerateCloner(t, false));
	}

   private static object GenerateCloner(Type t, bool asObject)
	{
		if (DeepClonerSafeTypes.CanReturnSameObject(t) && asObject && !t.IsValueType())
		{
			return null;
		}

		return DeepClonerExprGenerator.GenerateClonerInternal(t, asObject);
	}

 public static object CloneObjectTo(object objFrom, object objTo, bool isDeep)
	{
		if (objTo == null)
		{
			return null;
		}

		if (objFrom == null)
		{
			throw new ArgumentNullException(nameof(objFrom), "Cannot copy null object to another");
		}
		var type = objFrom.GetType();
		if (!type.IsInstanceOfType(objTo))
		{
			throw new InvalidOperationException("From object should be derived from From object, but From object has type " + objFrom.GetType().FullName + " and to " + objTo.GetType().FullName);
		}
		if (objFrom is string)
		{
			throw new InvalidOperationException("It is forbidden to clone strings");
		}
		var cloner = (Func<object, object, DeepCloneState, object>)(isDeep
			? DeepClonerCache.GetOrAddDeepClassTo(type, t => ClonerToExprGenerator.GenerateClonerInternal(t, true))
			: DeepClonerCache.GetOrAddShallowClassTo(type, t => ClonerToExprGenerator.GenerateClonerInternal(t, false)));
		if (cloner == null)
		{
			return objTo;
		}
		return cloner(objFrom, objTo, new DeepCloneState());
	}
}
