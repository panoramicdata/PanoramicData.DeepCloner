#nullable disable

using Xunit;
using PanoramicData.DeepCloner.Test.Objects;
using System;

namespace PanoramicData.DeepCloner.Test;

public class ShallowClonerSpec() : BaseTest(true)
{
	[Fact]
	public void SimpleObject_Should_Be_Cloned()
	{
		var cloned = TestObject1Sample.Create().ShallowClone();
		TestObject1Sample.AssertMatches(cloned);
	}

	private class C1
	{
		public object X { get; set; }
	}

	[Fact]
	public void Reference_Should_Not_Be_Copied()
	{
		var c1 = new C1
		{
			X = new object()
		};
		var clone = c1.ShallowClone();
		Assert.Equal(c1.X, clone.X);
	}

	private record struct S1 : IDisposable
	{
		public int X;

		public void Dispose()
		{
		}
	}

	[Fact]
	public void Struct_Should_Be_Cloned()
	{
		var c1 = new S1
		{
			X = 1
		};
		var clone = c1.ShallowClone();
		c1.X = 2;
		Assert.Equal(1, clone.X);
	}

	[Fact]
	public void Struct_As_Object_Should_Be_Cloned()
	{
		var c1 = new S1
		{
			X = 1
		};
		var clone = (S1)((IDisposable)c1).ShallowClone();
		c1.X = 2;
		Assert.Equal(1, clone.X);
	}

	// A boxed struct whose counter has already been advanced twice.
	private static IDoable CreateAdvancedDoable()
	{
		var c1 = new DoableStruct1() as IDoable;
		Assert.Equal(1, c1.Do());
		Assert.Equal(2, c1.Do());
		return c1;
	}

	private static Tuple<IDoable> CreateAdvancedTuple() => new(CreateAdvancedDoable());

	[Fact]
	public void Struct_As_Interface_Should_Be_Cloned()
	{
		var c1 = CreateAdvancedDoable();
		var clone = c1.ShallowClone();
		Assert.Equal(3, c1.Do());
		Assert.Equal(3, clone.Do());
	}

	[Fact]
	public void Struct_As_Interface_Should_Be_Cloned_For_DeepClone_Too()
	{
		var c1 = CreateAdvancedDoable();
		var clone = c1.DeepClone();
		Assert.Equal(3, c1.Do());
		Assert.Equal(3, clone.Do());
	}

	[Fact]
	public void Struct_As_Interface_Should_Be_Cloned_In_Object()
	{
		var t = CreateAdvancedTuple();
		var clone = t.ShallowClone();
		Assert.Equal(3, t.Item1.Do());
		// shallow clone do not copy object
		Assert.Equal(4, clone.Item1.Do());
	}

	[Fact]
	public void Struct_As_Interface_Should_Be_Cloned_For_DeepClone_Too_In_Object()
	{
		var t = CreateAdvancedTuple();
		var clone = t.DeepClone();
		Assert.Equal(3, t.Item1.Do());
		// deep clone copy object
		Assert.Equal(3, clone.Item1.Do());
	}

	[Fact]
	public void Primitive_Should_Be_Cloned()
	{
		Assert.Null(((object)null).ShallowClone());
		Assert.Equal(3, 3.ShallowClone());
	}

	[Fact]
	public void Array_Should_Be_Cloned()
	{
		var a = new[] { 3, 4 };
		var clone = a.ShallowClone();
		Assert.Equal(2, clone.Length);
		Assert.Equal(3, clone[0]);
		Assert.Equal(4, clone[1]);
	}
}
