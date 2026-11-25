#pragma warning disable S1104 // Public fields are deliberate: these fixtures exist to test field cloning
#nullable disable

using Xunit;

namespace PanoramicData.DeepCloner.Test;

public partial class ArraysSpec
{
	private static void AssertFlatEqualButNotSame(Array arr, Array clone)
	{
		Assert.False(ReferenceEquals(arr, clone));
		Assert.Equal(arr.Cast<object>(), clone.Cast<object>());
	}

	[Fact]
	public void MultiDim_Array_Should_Be_Cloned()
	{
		var arr = new int[,] { { 1, 2 }, { 3, 4 } };
		AssertFlatEqualButNotSame(arr, arr.DeepClone());
	}

	[Fact]
	public void MultiDim_Array_Should_Be_Cloned2()
	{
		var arr = new int[2, 2, 1];
		arr[0, 0, 0] = 1;
		arr[0, 1, 0] = 2;
		arr[1, 0, 0] = 3;
		arr[1, 1, 0] = 4;
		AssertFlatEqualButNotSame(arr, arr.DeepClone());
	}

	[Fact]
	public void MultiDim_Array_Should_Be_Cloned3()
	{
		var arr = new int[4, 5, 6];
		var indices = from i1 in Enumerable.Range(0, 4) from i2 in Enumerable.Range(0, 5) from i3 in Enumerable.Range(0, 6) select (i1, i2, i3);
		foreach (var (i1, i2, i3) in indices)
		{
			arr[i1, i2, i3] = i1 * 100 + i2 * 10 + i3;
		}

		var clone = arr.DeepClone();
		AssertFlatEqualButNotSame(arr, clone);
		Assert.All(indices, i => Assert.Equal(i.i1 * 100 + i.i2 * 10 + i.i3, clone[i.i1, i.i2, i.i3]));
	}

	[Fact]
	public void MultiDim_Array_Of_Classes_Should_Be_Cloned()
	{
		var arr = new AC[2, 2];
		arr[0, 0] = arr[1, 1] = new AC();
		var clone = arr.DeepClone();
		Assert.NotNull(clone[0, 0]);
		Assert.NotNull(clone[1, 1]);
		Assert.Equal(clone[0, 0], clone[1, 1]);
		Assert.NotEqual(arr[0, 0], clone[1, 1]);
	}

	[Fact]
	public void NonZero_Based_Array_Should_Be_Cloned()
	{
		var arr = Array.CreateInstance(typeof(int), [2], [1]);
		arr.SetValue(1, 1);
		arr.SetValue(2, 2);
		var clone = arr.DeepClone();
		Assert.Equal([1, 2], new[] { (int)clone.GetValue(1), (int)clone.GetValue(2) });
	}

	[Fact]
	public void NonZero_Based_MultiDim_Array_Should_Be_Cloned()
	{
		var arr = Array.CreateInstance(typeof(int), [2, 2], [1, 1]);
		arr.SetValue(1, 1, 1);
		arr.SetValue(2, 2, 2);
		var clone = arr.DeepClone();
		Assert.Equal([1, 2], new[] { (int)clone.GetValue(1, 1), (int)clone.GetValue(2, 2) });
	}

	[Fact]
	public void Array_As_Generic_Array_Should_Be_Cloned()
		=> Assert.Equal([1, 2, 3], (int[])((Array)new[] { 1, 2, 3 }).DeepClone());

	[Fact]
	public void Array_As_IEnumerable_Should_Be_Cloned()
		=> Assert.Equal([1, 2, 3], (int[])((IEnumerable<int>)new[] { 1, 2, 3 }).DeepClone());

	[Fact]
	public void MultiDimensional_Array_Should_Be_Cloned()
	{
		// Issue #25
		int[][] shapes = [[0, 0], [1, 0], [0, 1], [1, 1], [0, 0, 0], [1, 0, 0], [0, 1, 0], [0, 0, 1], [1, 1, 1]];
		foreach (var shape in shapes)
		{
			Array.CreateInstance(typeof(int), shape).DeepClone();
		}
	}
}
