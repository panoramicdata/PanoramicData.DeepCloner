#if !NETCORE
using System;
using System.Reflection;

namespace PanoramicData.DeepCloner.Helpers;

internal static class DeepClonerMsilHelper
{
	public static bool IsConstructorDoNothing(Type type, ConstructorInfo constructor)
	{
		if (constructor == null)
		{
			return false;
		}
		try
		{
			// will not try to determine body for this types
			if (IsSkippableType(type))
			{
				return false;
			}

			var methodBody = constructor.GetMethodBody();

			// this situation can be for com
			if (methodBody == null)
			{
				return false;
			}

			var ilAsByteArray = methodBody.GetILAsByteArray();
			if (IsCallObjectCtor(ilAsByteArray, type))
			{
				return true;
			}
			if (IsEmptyCtor(ilAsByteArray))
			{
				return true;
			}

			return false;
		}
		catch (Exception)
		{
			// no permissions or something similar
			return false;
		}
	}

	private static bool IsSkippableType(Type type)
	{
		return type.IsGenericType
			|| type.IsContextful
			|| type.IsCOMObject
			|| type.Assembly.IsDynamic;
	}

	private static bool IsCallObjectCtor(byte[]? ilAsByteArray, Type type)
	{
		if (ilAsByteArray == null || ilAsByteArray.Length != 7)
		{
			return false;
		}
		return ilAsByteArray[0] == 0x02 // Ldarg_0
			&& ilAsByteArray[1] == 0x28 // newobj
			&& ilAsByteArray[6] == 0x2a // ret
			&& type.Module.ResolveMethod(BitConverter.ToInt32(ilAsByteArray, 2)) == typeof(object).GetConstructor(Type.EmptyTypes); // call object
	}

	private static bool IsEmptyCtor(byte[]? ilAsByteArray)
	{
		return ilAsByteArray != null && ilAsByteArray.Length == 1 && ilAsByteArray[0] == 0x2a; // ret
	}
}
#endif