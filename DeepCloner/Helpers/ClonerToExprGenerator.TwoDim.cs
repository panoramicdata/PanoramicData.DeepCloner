#nullable disable

using System.Linq.Expressions;
using System.Reflection;

namespace PanoramicData.DeepCloner.Helpers;

internal static partial class ClonerToExprGenerator
{
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
