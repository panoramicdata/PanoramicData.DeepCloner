#pragma warning disable S1104 // Public fields are deliberate: these fixtures exist to test field cloning
#nullable disable

using Xunit;
using PanoramicData.DeepCloner;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PanoramicData.DeepCloner.Test;

public partial class ArraysSpec() : BaseTest(true)
{
	private static T[] CloneAndAssertEqual<T>(T[] arr)
	{
		var cloned = arr.DeepClone();
		Assert.Equal(arr, cloned);
		return cloned;
	}

	[Fact]
	public void IntArray_Should_Be_Cloned() => Assert.Equal(3, CloneAndAssertEqual(new[] { 1, 2, 3 }).Length);

	[Fact]
	public void StringArray_Should_Be_Cloned() => Assert.Equal(3, CloneAndAssertEqual(new[] { "1", "2", "3" }).Length);

	[Fact]
	public void StringArray_Should_Be_Cloned_Two_Arrays()
	{
		// checking that cached object correctly clones arrays of different length
		var arr = new[] { "111111111111111111111", "2", "3" };
		var cloned = CloneAndAssertEqual(arr);
		// strings should not be copied
		Assert.True(ReferenceEquals(arr[1], cloned[1]));

		Assert.Equal(4, CloneAndAssertEqual(["1", "2", "3", "4"]).Length);
		Assert.Empty(CloneAndAssertEqual<string>([]));

		var nullArray = (string[])null;
		Assert.Null(nullArray.DeepClone());
	}

	[Fact]
	public void StringArray_Casted_As_Object_Should_Be_Cloned()
	{
		var arr = new[] { "1", "2", "3" };
		var cloned = ((object)arr).DeepClone() as string[];
		Assert.Equal(arr, cloned);
		// strings should not be copied
		Assert.True(ReferenceEquals(arr[1], cloned[1]));
	}

	[Fact]
	public void ByteArray_Should_Be_Cloned()
	{
		CloneAndAssertEqual(Encoding.ASCII.GetBytes("test"));
		CloneAndAssertEqual(Encoding.ASCII.GetBytes(string.Concat(Enumerable.Repeat("test", 30))));
	}

	[Fact]
	public void IntList_Should_Be_Cloned()
		=> Assert.Equal([1, 2, 3], new List<int> { 1, 2, 3 }.DeepClone());

	[Fact]
	public void Dictionary_Should_Be_Cloned()
	{
		var d = new Dictionary<string, decimal> { ["a"] = 1, ["b"] = 2 };
		Assert.Equal(d.OrderBy(p => p.Key), d.DeepClone().OrderBy(p => p.Key));
	}

	[Fact]
	public void Array_Of_Same_Arrays_Should_Be_Cloned()
	{
		var c1 = new[] { 1, 2, 3 };
		var arr = new[] { c1, c1, c1, c1, c1 };
		var cloned = arr.DeepClone();

		Assert.Equal(5, cloned.Length);
		// lot of objects for checking reference dictionary optimization
		Assert.False(ReferenceEquals(arr[0], cloned[0]));
		Assert.All(cloned, item => Assert.True(ReferenceEquals(cloned[0], item)));
	}

	public class AC
	{
		public int[] A { get; set; }

		public int[] B { get; set; }
	}

	[Fact]
	public void Class_With_Same_Arrays_Should_Be_Cloned()
	{
		var ac = new AC();
		ac.A = ac.B = new int[3];
		var clone = ac.DeepClone();
		Assert.False(ReferenceEquals(ac.A, clone.A));
		Assert.True(ReferenceEquals(clone.A, clone.B));
	}

	[Fact]
	public void Class_With_Null_Array_hould_Be_Cloned()
	{
		var cloned = new AC().DeepClone();
		Assert.Null(cloned.A);
		Assert.Null(cloned.B);
	}
}
