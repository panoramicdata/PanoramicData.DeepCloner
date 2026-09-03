#nullable disable

using Xunit;
using PanoramicData.DeepCloner;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PanoramicData.DeepCloner.Test;

public class ArraysSpec() : BaseTest(true)
{
	[Fact]
	public void IntArray_Should_Be_Cloned()
	{
		var arr = new[] { 1, 2, 3 };
		var cloned = arr.DeepClone();
		Assert.Equal(3, cloned.Length);
		Assert.Equal(arr, cloned);
	}

	[Fact]
	public void StringArray_Should_Be_Cloned()
	{
		var arr = new[] { "1", "2", "3" };
		var cloned = arr.DeepClone();
		Assert.Equal(3, cloned.Length);
		Assert.Equal(arr, cloned);
	}

	[Fact]
	public void StringArray_Should_Be_Cloned_Two_Arrays()
	{
		// checking that cached object correctly clones arrays of different length
		var arr = new[] { "111111111111111111111", "2", "3" };
		var cloned = arr.DeepClone();
		Assert.Equal(3, cloned.Length);
		Assert.Equal(arr, cloned);
		// strings should not be copied
		Assert.True(ReferenceEquals(arr[1], cloned[1]));

		arr = ["1", "2", "3", "4"];
		cloned = arr.DeepClone();
		Assert.Equal(4, cloned.Length);
		Assert.Equal(arr, cloned);

		arr = [];
		cloned = arr.DeepClone();
		Assert.Empty(cloned);

		if (1.Equals(1)) arr = null;
		Assert.Null(arr.DeepClone());
	}

	[Fact]
	public void StringArray_Casted_As_Object_Should_Be_Cloned()
	{
		// checking that cached object correctly clones arrays of different length
		var arr = (object)new[] { "1", "2", "3" };
		var cloned = arr.DeepClone() as string[];
		Assert.Equal(3, cloned.Length);
		Assert.Equal((string[])arr, cloned);
		// strings should not be copied
		Assert.True(ReferenceEquals(((string[])arr)[1], cloned[1]));
	}

	[Fact]
	public void ByteArray_Should_Be_Cloned()
	{
		// checking that cached object correctly clones arrays of different length
		var arr = Encoding.ASCII.GetBytes("test");
		var cloned = arr.DeepClone();
		Assert.Equal(arr, cloned);

		arr = Encoding.ASCII.GetBytes("test testtest testtest testtest testtest testtest testtest testtest testtest testtest testtest testtest testtest testte");
		cloned = arr.DeepClone();
		Assert.Equal(arr, cloned);
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
		Assert.Equal(2, cloned.Length);
		Assert.Equal(1, cloned[0].X);
		Assert.Equal(2, cloned[1].X);
		Assert.NotEqual(arr[0], cloned[0]);
		Assert.NotEqual(arr[1], cloned[1]);
	}

	public struct S1(int x)
	{
		public int X = x;
	}

	public struct S2
	{
		public C1 C;
	}

	[Fact]
	public void StructArray_Should_Be_Cloned()
	{
		var arr = new[] { new S1(1), new S1(2) };
		var cloned = arr.DeepClone();
		Assert.Equal(2, cloned.Length);
		Assert.Equal(1, cloned[0].X);
		Assert.Equal(2, cloned[1].X);
	}

	[Fact]
	public void StructArray_With_Class_Should_Be_Cloned()
	{
		var arr = new[] { new S2 { C = new C1(1) }, new S2 { C = new C1(2) } };
		var cloned = arr.DeepClone();
		Assert.Equal(2, cloned.Length);
		Assert.Equal(1, cloned[0].C.X);
		Assert.Equal(2, cloned[1].C.X);
		Assert.NotEqual(arr[0].C, cloned[0].C);
		Assert.NotEqual(arr[1].C, cloned[1].C);
	}

	[Fact]
	public void NullArray_Should_Be_Cloned()
	{
		var arr = new C1[] { null, null };
		var cloned = arr.DeepClone();
		Assert.Equal(2, cloned.Length);
		Assert.Null(cloned[0]);
		Assert.Null(cloned[1]);
	}

	[Fact]
	public void NullAsArray_hould_Be_Cloned()
	{
		var arr = (int[])null;
		// ReSharper disable ExpressionIsAlwaysNull
		var cloned = arr.DeepClone();
		// ReSharper restore ExpressionIsAlwaysNull
		Assert.Null(cloned);
	}

	[Fact]
	public void IntList_Should_Be_Cloned()
	{
		// TODO: better performance for this type
		var arr = new List<int> { 1, 2, 3 };
		var cloned = arr.DeepClone();
		Assert.Equal(3, cloned.Count);
		Assert.Equal(1, cloned[0]);
		Assert.Equal(2, cloned[1]);
		Assert.Equal(3, cloned[2]);
	}

	[Fact]
	public void Dictionary_Should_Be_Cloned()
	{
		// TODO: better performance for this type
		var d = new Dictionary<string, decimal>
		{
			["a"] = 1,
			["b"] = 2
		};
		var cloned = d.DeepClone();
		Assert.Equal(2, cloned.Count);
		Assert.Equal(1, cloned["a"]);
		Assert.Equal(2, cloned["b"]);
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
		Assert.True(ReferenceEquals(cloned[0], cloned[1]));
		Assert.True(ReferenceEquals(cloned[1], cloned[2]));
		Assert.True(ReferenceEquals(cloned[1], cloned[3]));
		Assert.True(ReferenceEquals(cloned[1], cloned[4]));
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
		var ac = new AC();
		var cloned = ac.DeepClone();
		Assert.Null(cloned.A);
		Assert.Null(cloned.B);
	}

	[Fact]
	public void MultiDim_Array_Should_Be_Cloned()
	{
		var arr = new int[2, 2];
		arr[0, 0] = 1;
		arr[0, 1] = 2;
		arr[1, 0] = 3;
		arr[1, 1] = 4;
		var clone = arr.DeepClone();
		Assert.False(ReferenceEquals(arr, clone));
		Assert.Equal(1, clone[0, 0]);
		Assert.Equal(2, clone[0, 1]);
		Assert.Equal(3, clone[1, 0]);
		Assert.Equal(4, clone[1, 1]);
	}

	[Fact]
	public void MultiDim_Array_Should_Be_Cloned2()
	{
		var arr = new int[2, 2, 1];
		arr[0, 0, 0] = 1;
		arr[0, 1, 0] = 2;
		arr[1, 0, 0] = 3;
		arr[1, 1, 0] = 4;
		var clone = arr.DeepClone();
		Assert.False(ReferenceEquals(arr, clone));
		Assert.Equal(1, clone[0, 0, 0]);
		Assert.Equal(2, clone[0, 1, 0]);
		Assert.Equal(3, clone[1, 0, 0]);
		Assert.Equal(4, clone[1, 1, 0]);
	}

	[Fact]
	public void MultiDim_Array_Should_Be_Cloned3()
	{
		const int cnt1 = 4;
		const int cnt2 = 5;
		const int cnt3 = 6;
		var arr = new int[cnt1, cnt2, cnt3];
		for (var i1 = 0; i1 < cnt1; i1++)
			for (var i2 = 0; i2 < cnt2; i2++)
				for (var i3 = 0; i3 < cnt3; i3++)
					arr[i1, i2, i3] = i1 * 100 + i2 * 10 + i3;
		var clone = arr.DeepClone();
		Assert.False(ReferenceEquals(arr, clone));
		for (var i1 = 0; i1 < cnt1; i1++)
			for (var i2 = 0; i2 < cnt2; i2++)
				for (var i3 = 0; i3 < cnt3; i3++)
					Assert.Equal(i1 * 100 + i2 * 10 + i3, arr[i1, i2, i3]);
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
		Assert.Equal(1, clone.GetValue(1));
		Assert.Equal(2, clone.GetValue(2));
	}

	[Fact]
	public void NonZero_Based_MultiDim_Array_Should_Be_Cloned()
	{
		var arr = Array.CreateInstance(typeof(int), [2, 2], [1, 1]);

		arr.SetValue(1, 1, 1);
		arr.SetValue(2, 2, 2);
		var clone = arr.DeepClone();
		Assert.Equal(1, clone.GetValue(1, 1));
		Assert.Equal(2, clone.GetValue(2, 2));
	}

	[Fact]
	public void Array_As_Generic_Array_Should_Be_Cloned()
	{
		var arr = new[] { 1, 2, 3 };
		var genArr = (Array)arr;
		var clone = (int[])genArr.DeepClone();
		Assert.Equal(3, clone.Length);
		Assert.Equal(1, clone[0]);
		Assert.Equal(2, clone[1]);
		Assert.Equal(3, clone[2]);
	}

	[Fact]
	public void Array_As_IEnumerable_Should_Be_Cloned()
	{
		var arr = new[] { 1, 2, 3 };
		var genArr = (IEnumerable<int>)arr;
		var clone = (int[])genArr.DeepClone();
		// ReSharper disable PossibleMultipleEnumeration
		Assert.Equal(3, clone.Length);
		Assert.Equal(1, clone[0]);
		Assert.Equal(2, clone[1]);
		Assert.Equal(3, clone[2]);
		// ReSharper restore PossibleMultipleEnumeration
	}

	[Fact]
	public void MultiDimensional_Array_Should_Be_Cloned()
	{
		// Issue #25
		Array.CreateInstance(typeof(int), [0, 0]).DeepClone();
		Array.CreateInstance(typeof(int), [1, 0]).DeepClone();
		Array.CreateInstance(typeof(int), [0, 1]).DeepClone();
		Array.CreateInstance(typeof(int), [1, 1]).DeepClone();

		Array.CreateInstance(typeof(int), [0, 0, 0]).DeepClone();
		Array.CreateInstance(typeof(int), [1, 0, 0]).DeepClone();
		Array.CreateInstance(typeof(int), [0, 1, 0]).DeepClone();
		Array.CreateInstance(typeof(int), [0, 0, 1]).DeepClone();
		Array.CreateInstance(typeof(int), [1, 1, 1]).DeepClone();
	}

	[Fact]
	public void Issue_17_Spec()
	{
		// Deliberately asserting through each set's own Contains: it honours the set's
		// comparer, which is what this regression test is about. Assert.Contains would
		// use the default comparer instead, so xUnit2017 is suppressed here.
#pragma warning disable xUnit2017
		var set = new HashSet<string> { "value" };
		Assert.True(set.Contains("value"));

		var cloned = set.DeepClone();
		Assert.True(cloned.Contains("value"));

		var copyOfSet = new HashSet<string>(set, set.Comparer);
		Assert.True(copyOfSet.Contains("value"));

		var copyOfCloned = new HashSet<string>(cloned, cloned.Comparer);
		Assert.True(copyOfCloned.ToArray()[0] == "value");

		Assert.True(copyOfCloned.Contains("value"));
#pragma warning restore xUnit2017
	}

	[Fact]
	public void Check_Comparer_Cloning()
	{
		Check_Comparer_does_Clone_Internal<string>();

		Check_Comparer_does_not_Clone_Internal<int>();
		Check_Comparer_does_not_Clone_Internal<object>();
		Check_Comparer_does_not_Clone_Internal<FileShare>();
		Check_Comparer_does_not_Clone_Internal<byte[]>();
		Check_Comparer_does_not_Clone_Internal<byte>();
		Check_Comparer_does_not_Clone_Internal<int?>();
		Check_Comparer_does_not_Clone_Internal<HashSet<int>>();

		Assert.True(StringComparer.Ordinal == StringComparer.Ordinal.DeepClone());
		Assert.True(StringComparer.InvariantCulture == StringComparer.InvariantCulture.DeepClone());
		Assert.True(StringComparer.InvariantCultureIgnoreCase == StringComparer.InvariantCultureIgnoreCase.DeepClone());

		Assert.False(StringComparer.OrdinalIgnoreCase == StringComparer.OrdinalIgnoreCase.DeepClone());
		Assert.False(StringComparer.CurrentCulture == StringComparer.CurrentCulture.DeepClone());
		Assert.False(StringComparer.CurrentCultureIgnoreCase == StringComparer.CurrentCultureIgnoreCase.DeepClone());
	}

	private void Check_Comparer_does_Clone_Internal<T>()
	{
		var comparer = EqualityComparer<T>.Default;
		var cloned = comparer.DeepClone();

		// checking by reference
		Assert.False(comparer == cloned);
	}

	private void Check_Comparer_does_not_Clone_Internal<T>()
	{
		var comparer = EqualityComparer<T>.Default;
		var cloned = comparer.DeepClone();

		// checking by reference
		Assert.True(comparer == cloned);
	}
}
