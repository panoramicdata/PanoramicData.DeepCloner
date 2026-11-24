#nullable disable

using Xunit;
using PanoramicData.DeepCloner;

namespace PanoramicData.DeepCloner.Test;

public partial class CopyToObjectSpec
{
	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void Non_Zero_Based_Array_Should_Be_Cloned(bool isDeep)
	{
		var arrFrom = Array.CreateInstance(typeof(int), [2], [1]);
		// with offset. its ok
		var arrTo = Array.CreateInstance(typeof(int), [2], [0]);
		arrFrom.SetValue(1, 1);
		arrFrom.SetValue(2, 2);
		if (isDeep)
		{
			arrFrom.DeepCloneTo(arrTo);
		}
		else
		{
			arrFrom.ShallowCloneTo(arrTo);
		}
		Assert.Equal(2, arrTo.Length);
		Assert.Equal(1, arrTo.GetValue(0));
		Assert.Equal(2, arrTo.GetValue(1));
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void MultiDim_Array_Should_Be_Cloned(bool isDeep)
	{
		var arrFrom = Array.CreateInstance(typeof(int), [2, 2], [1, 1]);
		// with offset. its ok
		var arrTo = Array.CreateInstance(typeof(int), [1, 1], [0, 0]);
		arrFrom.SetValue(1, 1, 1);
		arrFrom.SetValue(2, 2, 2);
		if (isDeep)
		{
			arrFrom.DeepCloneTo(arrTo);
		}
		else
		{
			arrFrom.ShallowCloneTo(arrTo);
		}
		// The point is that the destination array's own dimensions were not resized,
		// so assert Length directly rather than Assert.Single (hence xUnit2013).
#pragma warning disable xUnit2013
		Assert.Equal(1, arrTo.Length);
#pragma warning restore xUnit2013
		Assert.Equal(1, arrTo.GetValue(0, 0));
	}

	public readonly record struct StructWithReference(int[] Items);

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void TwoDim_Array_Of_Structs_With_References_Should_Be_Cloned(bool isDeep)
	{
		var arrFrom = new[,]
		{
			{ new StructWithReference([1]), new StructWithReference([2]) },
			{ new StructWithReference([3]), new StructWithReference([4]) }
		};
		var arrTo = new StructWithReference[2, 2];

		if (isDeep)
		{
			arrFrom.DeepCloneTo(arrTo);
		}
		else
		{
			arrFrom.ShallowCloneTo(arrTo);
		}

		Assert.Equal([4], arrTo[1, 1].Items);
		Assert.Equal([1], arrTo[0, 0].Items);
		Assert.Equal([3], arrTo[1, 0].Items);
		Assert.Equal(isDeep, !ReferenceEquals(arrFrom[1, 1].Items, arrTo[1, 1].Items));
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void TwoDim_Array_Should_Be_Cloned(bool isDeep)
	{
		var arrFrom = new[,] { { 1, 2 }, { 3, 4 } };
		// with offset. its ok
		var arrTo = new int[3, 1];
		if (isDeep)
		{
			arrFrom.DeepCloneTo(arrTo);
		}
		else
		{
			arrFrom.ShallowCloneTo(arrTo);
		}
		Assert.Equal(1, arrTo[0, 0]);
		Assert.Equal(3, arrTo[1, 0]);

		arrTo = new int[2, 2];
		if (isDeep)
		{
			arrFrom.DeepCloneTo(arrTo);
		}
		else
		{
			arrFrom.ShallowCloneTo(arrTo);
		}
		Assert.Equal(1, arrTo[0, 0]);
		Assert.Equal(2, arrTo[0, 1]);
		Assert.Equal(3, arrTo[1, 0]);
	}

	[Fact]
	public void MultiDim_Array_Should_Be_Cloned3()
	{
		const int cnt1 = 4;
		const int cnt2 = 5;
		const int cnt3 = 6;
		var arr = new int[cnt1, cnt2, cnt3];
		for (var i1 = 0; i1 < cnt1; i1++)
		{
			for (var i2 = 0; i2 < cnt2; i2++)
			{
				for (var i3 = 0; i3 < cnt3; i3++)
				{
					arr[i1, i2, i3] = i1 * 100 + i2 * 10 + i3;
				}
			}
		}
		var clone = arr.DeepCloneTo(new int[cnt1, cnt2, cnt3]);
		Assert.False(ReferenceEquals(arr, clone));
		for (var i1 = 0; i1 < cnt1; i1++)
		{
			for (var i2 = 0; i2 < cnt2; i2++)
			{
				for (var i3 = 0; i3 < cnt3; i3++)
				{
					Assert.Equal(i1 * 100 + i2 * 10 + i3, arr[i1, i2, i3]);
				}
			}
		}
	}

	[Fact]
	public void MultiDimensional_Array_Should_Be_Cloned()
	{
		// Issue #25
		Array.CreateInstance(typeof(int), [0, 0]).DeepCloneTo(new int[0, 0]);
		Array.CreateInstance(typeof(int), [1, 0]).DeepCloneTo(new int[1, 0]);
		Array.CreateInstance(typeof(int), [0, 1]).DeepCloneTo(new int[0, 1]);
		Array.CreateInstance(typeof(int), [1, 1]).DeepCloneTo(new int[1, 1]);

		Array.CreateInstance(typeof(int), [0, 0, 0]).DeepCloneTo(new int[0, 0, 0]);
		Array.CreateInstance(typeof(int), [1, 0, 0]).DeepCloneTo(new int[1, 0, 0]);
		Array.CreateInstance(typeof(int), [0, 1, 0]).DeepCloneTo(new int[0, 1, 0]);
		Array.CreateInstance(typeof(int), [0, 0, 1]).DeepCloneTo(new int[0, 0, 1]);
		Array.CreateInstance(typeof(int), [1, 1, 1]).DeepCloneTo(new int[1, 1, 1]);
	}

	[Fact]
	public void Shallow_Clone_Of_MultiDim_Array_Should_Not_Perform_Deep()
	{
		var c1 = new C1();
		var arrFrom = new[,] { { c1, c1 }, { c1, c1 } };
		// with offset. its ok
		var arrTo = new C1[3, 1];
		arrFrom.ShallowCloneTo(arrTo);
		Assert.True(ReferenceEquals(c1, arrTo[0, 0]));
		Assert.True(ReferenceEquals(c1, arrTo[1, 0]));

		var arrFrom2 = new C1[1, 1, 1];
		arrFrom2[0, 0, 0] = c1;
		var arrTo2 = new C1[1, 1, 1];
		arrFrom2.ShallowCloneTo(arrTo2);
		Assert.True(ReferenceEquals(c1, arrTo2[0, 0, 0]));
	}

	[Fact]
	public void Deep_Clone_Of_MultiDim_Array_Should_Perform_Deep()
	{
		var c1 = new C1();
		var arrFrom = new[,] { { c1, c1 }, { c1, c1 } };
		// with offset. its ok
		var arrTo = new C1[3, 1];
		arrFrom.DeepCloneTo(arrTo);
		Assert.False(ReferenceEquals(c1, arrTo[0, 0]));
		Assert.True(ReferenceEquals(arrTo[0, 0], arrTo[1, 0]));

		var arrFrom2 = new C1[1, 1, 2];
		arrFrom2[0, 0, 0] = c1;
		arrFrom2[0, 0, 1] = c1;
		var arrTo2 = new C1[1, 1, 2];
		arrFrom2.DeepCloneTo(arrTo2);
		Assert.False(ReferenceEquals(c1, arrTo2[0, 0, 0]));
		Assert.True(ReferenceEquals(arrTo2[0, 0, 1], arrTo2[0, 0, 0]));
	}
}
