#nullable disable

using System;
using System.Linq;

namespace PanoramicData.DeepCloner.Helpers;

internal static partial class DeepClonerGenerator
{
	// when we can't use code generation, we can use these methods
	internal static T[] Clone1DimArraySafeInternal<T>(T[] obj, DeepCloneState state)
	{
		var l = obj.Length;
		var outArray = new T[l];
		state.AddKnownRef(obj, outArray);
		Array.Copy(obj, outArray, obj.Length);
		return outArray;
	}

	internal static T[] Clone1DimArrayStructInternal<T>(T[] obj, DeepCloneState state)
	{
		// not null from called method, but will check it anyway
		if (obj == null)
		{
			return null;
		}
		var l = obj.Length;
		var outArray = new T[l];
		state.AddKnownRef(obj, outArray);
		var cloner = GetClonerForValueType<T>();
		for (var i = 0; i < l; i++)
		{
			outArray[i] = cloner(obj[i], state);
		}

		return outArray;
	}

	internal static T[] Clone1DimArrayClassInternal<T>(T[] obj, DeepCloneState state)
	{
		// not null from called method, but will check it anyway
		if (obj == null)
		{
			return null;
		}
		var l = obj.Length;
		var outArray = new T[l];
		state.AddKnownRef(obj, outArray);
		for (var i = 0; i < l; i++)
		{
			outArray[i] = (T)CloneClassInternal(obj[i], state);
		}

		return outArray;
	}

	// relatively frequent case. specially handled
	internal static T[,] Clone2DimArrayInternal<T>(T[,] obj, DeepCloneState state)
	{
		// not null from called method, but will check it anyway
		if (obj == null)
		{
			return null;
		}

		// we cannot determine by type multidim arrays (one dimension is possible)
		// so, will check for index here
		var lb1 = obj.GetLowerBound(0);
		var lb2 = obj.GetLowerBound(1);
		if (lb1 != 0 || lb2 != 0)
		{
			return (T[,])CloneAbstractArrayInternal(obj, state);
		}

		var l1 = obj.GetLength(0);
		var l2 = obj.GetLength(1);
		var outArray = new T[l1, l2];
		state.AddKnownRef(obj, outArray);
		if (DeepClonerSafeTypes.CanReturnSameObject(typeof(T)))
		{
			Array.Copy(obj, outArray, obj.Length);
			return outArray;
		}

		if (typeof(T).IsValueType())
		{
			var cloner = GetClonerForValueType<T>();
			Fill2DimArray(outArray, obj, l1, l2, state, (item, s) => cloner(item, s));
		}
		else
		{
			Fill2DimArray(outArray, obj, l1, l2, state, (item, s) => (T)CloneClassInternal(item, s));
		}

		return outArray;
	}

	private static void Fill2DimArray<T>(T[,] outArray, T[,] source, int l1, int l2, DeepCloneState state, Func<T, DeepCloneState, T> cloner)
	{
		for (var i = 0; i < l1; i++)
		{
			for (var k = 0; k < l2; k++)
			{
				outArray[i, k] = cloner(source[i, k], state);
			}
		}
	}

	// rare cases, very slow cloning. currently it's ok
	internal static Array CloneAbstractArrayInternal(Array obj, DeepCloneState state)
	{
		// not null from called method, but will check it anyway
		if (obj == null)
		{
			return null;
		}
		var rank = obj.Rank;

		var lengths = Enumerable.Range(0, rank).Select(obj.GetLength).ToArray();

		var lowerBounds = Enumerable.Range(0, rank).Select(obj.GetLowerBound).ToArray();
		var idxes = Enumerable.Range(0, rank).Select(obj.GetLowerBound).ToArray();

		var elementType = obj.GetType().GetElementType();
		var outArray = Array.CreateInstance(elementType, lengths, lowerBounds);

		state.AddKnownRef(obj, outArray);

		// we're unable to set any value to this array, so, just return it
		if (lengths.Any(x => x == 0))
		{
			return outArray;
		}

		if (DeepClonerSafeTypes.CanReturnSameObject(elementType))
		{
			Array.Copy(obj, outArray, obj.Length);
			return outArray;
		}

		var ofs = rank - 1;
		while (true)
		{
			outArray.SetValue(CloneClassInternal(obj.GetValue(idxes), state), idxes);
			idxes[ofs]++;

			if (idxes[ofs] >= lowerBounds[ofs] + lengths[ofs])
			{
				do
				{
					if (ofs == 0)
					{
						return outArray;
					}
					idxes[ofs] = lowerBounds[ofs];
					ofs--;
					idxes[ofs]++;
				} while (idxes[ofs] >= lowerBounds[ofs] + lengths[ofs]);

				ofs = rank - 1;
			}
		}
	}
}
