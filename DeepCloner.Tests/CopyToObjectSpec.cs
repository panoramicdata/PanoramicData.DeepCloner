#nullable disable

using System;
using System.Collections.Generic;
using Xunit;
using PanoramicData.DeepCloner;

namespace PanoramicData.DeepCloner.Test;

public class CopyToObjectSpec
{
	public class C1
	{
		public int A { get; set; }

		public virtual string B { get; set; }

		public byte[] C { get; set; }
	}

	public class C2 : C1
	{
		public decimal D { get; set; }

		public new int A { get; set; }
	}

	public class C4 : C1
	{
	}

	public class C3
	{
		public C1 A { get; set; }

		public C1 B { get; set; }
	}

	public interface I1
	{
		int A { get; set; }
	}

	public struct S1 : I1
	{
		public int A { get; set; }
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void Simple_Class_Should_Be_Cloned(bool isDeep)
	{
		var cFrom = new C1
		{
			A = 12,
			B = "testestest",
			C = [1, 2, 3]
		};

		var cTo = new C1
		{
			A = 11,
			B = "tes",
			C = [1]
		};

		var cToRef = cTo;

		if (isDeep)
			cFrom.DeepCloneTo(cTo);
		else
			cFrom.ShallowCloneTo(cTo);

		Assert.True(ReferenceEquals(cTo, cToRef));
		Assert.Equal(12, cTo.A);
		Assert.Equal("testestest", cTo.B);
		Assert.Equal(3, cTo.C.Length);
		Assert.Equal(3, cTo.C[2]);
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void Descendant_Class_Should_Be_Cloned(bool isDeep)
	{
		var cFrom = new C1
		{
			A = 12,
			B = "testestest",
			C = [1, 2, 3]
		};

		var cTo = new C2
		{
			A = 11,
			D = 42.3m
		};

		var cToRef = cTo;

		if (isDeep)
			cFrom.DeepCloneTo(cTo);
		else
			cFrom.ShallowCloneTo(cTo);

		Assert.True(ReferenceEquals(cTo, cToRef));
		Assert.Equal(11, cTo.A);
		Assert.Equal(12, ((C1)cTo).A);
		Assert.Equal(42.3m, cTo.D);
	}

	[Fact]
	public void Class_With_Subclass_Should_Be_Shallow_CLoned()
	{
		var c1 = new C1 { A = 12 };
		var cFrom = new C3 { A = c1, B = c1 };
		var cTo = cFrom.ShallowCloneTo(new C3());
		Assert.True(ReferenceEquals(cFrom.A, cTo.A));
		Assert.True(ReferenceEquals(cFrom.B, cTo.B));
		Assert.True(ReferenceEquals(cTo.A, cTo.B));
	}

	[Fact]
	public void Class_With_Subclass_Should_Be_Deep_CLoned()
	{
		var c1 = new C1 { A = 12 };
		var cFrom = new C3 { A = c1, B = c1 };
		var cTo = cFrom.DeepCloneTo(new C3());
		Assert.False(ReferenceEquals(cFrom.A, cTo.A));
		Assert.False(ReferenceEquals(cFrom.B, cTo.B));
		Assert.True(ReferenceEquals(cTo.A, cTo.B));
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void Copy_To_Null_Should_Return_Null(bool isDeep)
	{
		var c1 = new C1();
		if (isDeep)
			Assert.Null(c1.DeepCloneTo((C1)null));
		else
			Assert.Null(c1.ShallowCloneTo((C1)null));
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void Copy_From_Null_Should_Throw_Error(bool isDeep)
	{
		C1 c1 = null;
		if (isDeep)
			// ReSharper disable once ExpressionIsAlwaysNull
			Assert.Throws<ArgumentNullException>(() => { _ = c1.DeepCloneTo(new C1()); });
		else
			// ReSharper disable once ExpressionIsAlwaysNull
			Assert.Throws<ArgumentNullException>(() => { _ = c1.ShallowCloneTo(new C1()); });
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void Invalid_Inheritance_Should_Throw_Error(bool isDeep)
	{
		C1 c1 = new C4();
		if (isDeep)
			// ReSharper disable once ExpressionIsAlwaysNull
			Assert.Throws<InvalidOperationException>(() => { _ = c1.DeepCloneTo(new C2()); });
		else
			// ReSharper disable once ExpressionIsAlwaysNull
			Assert.Throws<InvalidOperationException>(() => { _ = c1.ShallowCloneTo(new C2()); });
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void Struct_As_Interface_ShouldNot_Be_Cloned(bool isDeep)
	{
		S1 sFrom = new() { A = 42 };
		S1 sTo = new();
		var objTo = (I1)sTo;
		objTo.A = 23;
		if (isDeep)
			// ReSharper disable once ExpressionIsAlwaysNull
			Assert.Throws<InvalidOperationException>(() => { _ = ((I1)sFrom).DeepCloneTo(objTo); });
		else
			// ReSharper disable once ExpressionIsAlwaysNull
			Assert.Throws<InvalidOperationException>(() => { _ = ((I1)sFrom).ShallowCloneTo(objTo); });
	}

	[Fact]
	public void String_Should_Not_Be_Cloned()
	{
		var s1 = "abc";
		var s2 = "def";
		Assert.Throws<InvalidOperationException>(() => { _ = s1.ShallowCloneTo(s2); });
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void Array_Should_Be_Cloned_Correct_Size(bool isDeep)
	{
		var arrFrom = new[] { 1, 2, 3 };
		var arrTo = new[] { 4, 5, 6 };
		if (isDeep) arrFrom.DeepCloneTo(arrTo);
		else arrFrom.ShallowCloneTo(arrTo);
		Assert.Equal(3, arrTo.Length);
		Assert.Equal(1, arrTo[0]);
		Assert.Equal(2, arrTo[1]);
		Assert.Equal(3, arrTo[2]);
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void Array_Should_Be_Cloned_From_Is_Bigger(bool isDeep)
	{
		var arrFrom = new[] { 1, 2, 3 };
		var arrTo = new[] { 4, 5 };
		if (isDeep) arrFrom.DeepCloneTo(arrTo);
		else arrFrom.ShallowCloneTo(arrTo);
		Assert.Equal(2, arrTo.Length);
		Assert.Equal(1, arrTo[0]);
		Assert.Equal(2, arrTo[1]);
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void Array_Should_Be_Cloned_From_Is_Smaller(bool isDeep)
	{
		var arrFrom = new[] { 1, 2 };
		var arrTo = new[] { 4, 5, 6 };
		if (isDeep) arrFrom.DeepCloneTo(arrTo);
		else arrFrom.ShallowCloneTo(arrTo);
		Assert.Equal(3, arrTo.Length);
		Assert.Equal(1, arrTo[0]);
		Assert.Equal(2, arrTo[1]);
		Assert.Equal(6, arrTo[2]);
	}

	[Fact]
	public void Shallow_Array_Should_Be_Cloned()
	{
		var c1 = new C1();
		var arrFrom = new[] { c1, c1, c1 };
		var arrTo = new C1[4];
		arrFrom.ShallowCloneTo(arrTo);
		Assert.Equal(4, arrTo.Length);
		Assert.Equal(c1, arrTo[0]);
		Assert.Equal(c1, arrTo[1]);
		Assert.Equal(c1, arrTo[2]);
		Assert.Null(arrTo[3]);
	}

	[Fact]
	public void Deep_Array_Should_Be_Cloned()
	{
		var c1 = new C4();
		var c3 = new C3 { A = c1, B = c1 };
		var arrFrom = new[] { c3, c3, c3 };
		var arrTo = new C3[4];
		arrFrom.DeepCloneTo(arrTo);
		Assert.Equal(4, arrTo.Length);
		Assert.NotEqual<object>(c1, arrTo[0]);
		Assert.Equal(arrTo[1], arrTo[0]);
		Assert.Equal(arrTo[2], arrTo[1]);
		Assert.NotEqual(c1, arrTo[2].A);
		Assert.Equal(arrTo[2].B, arrTo[2].A);
		Assert.Null(arrTo[3]);
	}

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
		if (isDeep) arrFrom.DeepCloneTo(arrTo);
		else arrFrom.ShallowCloneTo(arrTo);
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
		if (isDeep) arrFrom.DeepCloneTo(arrTo);
		else arrFrom.ShallowCloneTo(arrTo);
		// The point is that the destination array's own dimensions were not resized,
		// so assert Length directly rather than Assert.Single (hence xUnit2013).
#pragma warning disable xUnit2013
		Assert.Equal(1, arrTo.Length);
#pragma warning restore xUnit2013
		Assert.Equal(1, arrTo.GetValue(0, 0));
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void TwoDim_Array_Should_Be_Cloned(bool isDeep)
	{
		var arrFrom = new[,] { { 1, 2 }, { 3, 4 } };
		// with offset. its ok
		var arrTo = new int[3, 1];
		if (isDeep) arrFrom.DeepCloneTo(arrTo);
		else arrFrom.ShallowCloneTo(arrTo);
		Assert.Equal(1, arrTo[0, 0]);
		Assert.Equal(3, arrTo[1, 0]);

		arrTo = new int[2, 2];
		if (isDeep) arrFrom.DeepCloneTo(arrTo);
		else arrFrom.ShallowCloneTo(arrTo);
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
			for (var i2 = 0; i2 < cnt2; i2++)
				for (var i3 = 0; i3 < cnt3; i3++)
					arr[i1, i2, i3] = i1 * 100 + i2 * 10 + i3;
		var clone = arr.DeepCloneTo(new int[cnt1, cnt2, cnt3]);
		Assert.False(ReferenceEquals(arr, clone));
		for (var i1 = 0; i1 < cnt1; i1++)
			for (var i2 = 0; i2 < cnt2; i2++)
				for (var i3 = 0; i3 < cnt3; i3++)
					Assert.Equal(i1 * 100 + i2 * 10 + i3, arr[i1, i2, i3]);
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

	[Fact]
	public void Dictionary_Should_Be_Deeply_Cloned()
	{
		var d1 = new Dictionary<string, string> { { "A", "B" }, { "C", "D" } };
		var d2 = new Dictionary<string, string>();
		d1.DeepCloneTo(d2);
		d1["A"] = "E";
		Assert.Equal(2, d2.Count);
		Assert.Equal("B", d2["A"]);
		Assert.Equal("D", d2["C"]);

		// big dictionary
		d1.Clear();
		for (var i = 0; i < 1000; i++)
			d1[i.ToString()] = i.ToString();
		d1.DeepCloneTo(d2);
		Assert.Equal(1000, d2.Count);
		Assert.Equal("557", d2["557"]);
	}

	public class D1
	{
		public int A { get; set; }
	}

	public class D2 : D1
	{
		public int B { get; set; }

		public D2(D1 d1)
		{
			B = 14;
			d1.DeepCloneTo(this);
		}
	}

	[Fact]
	public void Inner_Implementation_In_Class_Should_Work()
	{
		var baseObject = new D1 { A = 12 };
		var wrapper = new D2(baseObject);
		Assert.Equal(12, wrapper.A);
		Assert.Equal(14, wrapper.B);
	}
}