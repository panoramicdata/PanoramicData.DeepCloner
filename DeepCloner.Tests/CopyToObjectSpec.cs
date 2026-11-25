#nullable disable

using System;
using System.Collections.Generic;
using Xunit;
using PanoramicData.DeepCloner;

namespace PanoramicData.DeepCloner.Test;

public partial class CopyToObjectSpec
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

	public record struct S1 : I1
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
		{
			cFrom.DeepCloneTo(cTo);
		}
		else
		{
			cFrom.ShallowCloneTo(cTo);
		}

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
		{
			cFrom.DeepCloneTo(cTo);
		}
		else
		{
			cFrom.ShallowCloneTo(cTo);
		}

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
		{
			Assert.Null(c1.DeepCloneTo((C1)null));
		}
		else
		{
			Assert.Null(c1.ShallowCloneTo((C1)null));
		}
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void Copy_From_Null_Should_Throw_Error(bool isDeep)
	{
		C1 c1 = null;
		if (isDeep)
		{
			// ReSharper disable once ExpressionIsAlwaysNull
			Assert.Throws<ArgumentNullException>(() => { _ = c1.DeepCloneTo(new C1()); });
		}
		else
		{
			// ReSharper disable once ExpressionIsAlwaysNull
			Assert.Throws<ArgumentNullException>(() => { _ = c1.ShallowCloneTo(new C1()); });
		}
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void Invalid_Inheritance_Should_Throw_Error(bool isDeep)
	{
		C1 c1 = new C4();
		if (isDeep)
		{
			// ReSharper disable once ExpressionIsAlwaysNull
			Assert.Throws<InvalidOperationException>(() => { _ = c1.DeepCloneTo(new C2()); });
		}
		else
		{
			// ReSharper disable once ExpressionIsAlwaysNull
			Assert.Throws<InvalidOperationException>(() => { _ = c1.ShallowCloneTo(new C2()); });
		}
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
		{
			// ReSharper disable once ExpressionIsAlwaysNull
			Assert.Throws<InvalidOperationException>(() => { _ = ((I1)sFrom).DeepCloneTo(objTo); });
		}
		else
		{
			// ReSharper disable once ExpressionIsAlwaysNull
			Assert.Throws<InvalidOperationException>(() => { _ = ((I1)sFrom).ShallowCloneTo(objTo); });
		}
	}

	[Fact]
	public void String_Should_Not_Be_Cloned()
	{
		var s1 = "abc";
		var s2 = "def";
		Assert.Throws<InvalidOperationException>(() => { _ = s1.ShallowCloneTo(s2); });
	}
}
