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

	public class C1(int x)
	{
		public int X { get; set; } = x;
	}

	[Fact]
	public void ClassArray_Should_Be_Cloned()
	{
		var arr = new[] { new C1(1), new C1(2) };
		var cloned = arr.DeepClone();
		Assert.Equal([1, 2], cloned.Select(c => c.X));
		Assert.All(arr.Zip(cloned), pair => Assert.NotEqual(pair.First, pair.Second));
	}

	public struct S1(int x) : IEquatable<S1>
	{
		public int X = x;

		public readonly bool Equals(S1 other) => EqualityComparer<int>.Default.Equals(X, other.X);

		public override readonly bool Equals(object obj) => obj is S1 other && Equals(other);

		public override readonly int GetHashCode() => EqualityComparer<int>.Default.GetHashCode(X);
	}

	public struct S2 : IEquatable<S2>
	{
		public C1 C;

		public readonly bool Equals(S2 other) => EqualityComparer<C1>.Default.Equals(C, other.C);

		public override readonly bool Equals(object obj) => obj is S2 other && Equals(other);

		public override readonly int GetHashCode() => EqualityComparer<C1>.Default.GetHashCode(C);
	}

	[Fact]
	public void StructArray_Should_Be_Cloned()
		=> Assert.Equal([1, 2], new[] { new S1(1), new S1(2) }.DeepClone().Select(s => s.X));

	[Fact]
	public void StructArray_With_Class_Should_Be_Cloned()
	{
		var arr = new[] { new S2 { C = new C1(1) }, new S2 { C = new C1(2) } };
		var cloned = arr.DeepClone();
		Assert.Equal([1, 2], cloned.Select(s => s.C.X));
		Assert.All(arr.Zip(cloned), pair => Assert.NotEqual(pair.First.C, pair.Second.C));
	}

	[Fact]
	public void NullArray_Should_Be_Cloned()
	{
		var cloned = new C1[] { null, null }.DeepClone();
		Assert.Equal(2, cloned.Length);
		Assert.All(cloned, Assert.Null);
	}

	[Fact]
	public void NullAsArray_hould_Be_Cloned()
	{
		var arr = (int[])null;
		Assert.Null(arr.DeepClone());
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
