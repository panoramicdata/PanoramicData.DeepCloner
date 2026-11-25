#pragma warning disable S1104, S2357 // Public fields are deliberate: these fixtures exist to test field cloning
#nullable disable

using Xunit;
using PanoramicData.DeepCloner.Test.Objects;
using System;

namespace PanoramicData.DeepCloner.Test;

public partial class SimpleObjectSpec() : BaseTest(true)
{
	[Fact]
	public void SimpleObject_Should_Be_Cloned()
	{
		var cloned = TestObject1Sample.Create().DeepClone();
		TestObject1Sample.AssertMatches(cloned);
	}

	public record struct S1
	{
		public int A;
	}

	public record struct S2
	{
		public S3 S;
	}

	public record struct S3
	{
		public bool B;
	}

	// We have an special logic for simple structs, so, this test checks that this logic works correctly
	[Fact]
	public void SimpleStruct_Should_Be_Cloned()
	{
		var s1 = new S1 { A = 1 };
		var cloned = s1.DeepClone();
		Assert.Equal(1, cloned.A);
	}

	// We have an special logic for simple structs, so, this test checks that this logic works correctly
	[Fact]
	public void Simple_Struct_With_Child_Should_Be_Cloned()
	{
		var s1 = new S2 { S = new S3 { B = true } };
		var cloned = s1.DeepClone();
		Assert.True(cloned.S.B);
	}

	public class ClassWithNullable
	{
		public int? A { get; set; }

		public long? B { get; set; }
	}

	[Fact]
	public void Nullable_Should_Be_Cloned()
	{
		var c = new ClassWithNullable { B = 42 };
		var cloned = c.DeepClone();
		Assert.Null(cloned.A);
		Assert.Equal(42, cloned.B);
	}

	public class C1
	{
		public C2 C { get; set; }
	}

	public class C2
	{
	}

	public class C3
	{
		public string X { get; set; }
	}

	[Fact]
	public void Class_Should_Be_Cloned()
	{
		var c1 = new C1
		{
			C = new C2()
		};
		var cloned = c1.DeepClone();
		Assert.NotNull(cloned.C);
		Assert.NotEqual(c1.C, cloned.C);
	}

	public record struct S4
	{
		public C2 C;

		public int F;
	}

	[Fact]
	public void StructWithClass_Should_Be_Cloned()
	{
		var c1 = new S4
		{
			F = 1,
			C = new C2()
		};
		var cloned = c1.DeepClone();
		c1.F = 2;
		Assert.NotNull(cloned.C);
		Assert.Equal(1, cloned.F);
	}
}
