#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace PanoramicData.DeepCloner.Helpers;

internal static partial class ClonerToExprGenerator
{
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

	// shared start of the struct and class copies: null guard, copy length and loop registration
	private static bool TryBeginOneDimCopy<T>(T[] objFrom, T[] objTo, DeepCloneState state, out int length)
	{
		// not null from called method, but will check it anyway
		if (objFrom == null || objTo == null)
		{
			length = 0;
			return false;
		}

		length = Math.Min(objFrom.Length, objTo.Length);
		state.AddKnownRef(objFrom, objTo);
		return true;
	}

	internal static T[] Clone1DimArrayStructInternal<T>(T[] objFrom, T[] objTo, DeepCloneState state)
	{
		if (!TryBeginOneDimCopy(objFrom, objTo, state, out var l))
		{
			return null;
		}

		var cloner = DeepClonerGenerator.GetClonerForValueType<T>();
		for (var i = 0; i < l; i++)
		{
			objTo[i] = cloner(objTo[i], state);
		}

		return objTo;
	}

	internal static T[] Clone1DimArrayClassInternal<T>(T[] objFrom, T[] objTo, DeepCloneState state)
	{
		if (!TryBeginOneDimCopy(objFrom, objTo, state, out var l))
		{
			return null;
		}

		for (var i = 0; i < l; i++)
		{
			objTo[i] = (T)DeepClonerGenerator.CloneClassInternal(objFrom[i], state);
		}

		return objTo;
	}
}
