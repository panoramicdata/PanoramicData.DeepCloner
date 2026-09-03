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
		var obj = new TestObject1 { Int = 42, Byte = 42, Short = 42, Long = 42, DateTime = new DateTime(2001, 01, 01), Char = 'X', Decimal = 1.2m, Double = 1.3, Float = 1.4f, String = "test1", UInt = 42, ULong = 42, UShort = 42, Bool = true, IntPtr = new nint(42), UIntPtr = new nuint(42), Enum = AttributeTargets.Delegate };

		var cloned = obj.ShallowClone();
		Assert.Equal(42, cloned.Byte);
		Assert.Equal(42, cloned.Short);
		Assert.Equal(42, cloned.UShort);
		Assert.Equal(42, cloned.Int);
		Assert.Equal(42u, cloned.UInt);
		Assert.Equal(42, cloned.Long);
		Assert.Equal(42ul, cloned.ULong);
		Assert.Equal(1.2m, cloned.Decimal);
		Assert.Equal(1.3, cloned.Double);
		Assert.Equal(1.4f, cloned.Float);
		Assert.Equal('X', cloned.Char);
		Assert.Equal("test1", cloned.String);
		Assert.Equal(new DateTime(2001, 1, 1), cloned.DateTime);
		Assert.True(cloned.Bool);
		Assert.Equal(new nint(42), cloned.IntPtr);
		Assert.Equal(new nuint(42), cloned.UIntPtr);
		Assert.Equal(AttributeTargets.Delegate, cloned.Enum);
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

	private struct S1 : IDisposable
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

	[Fact]
	public void Struct_As_Interface_Should_Be_Cloned()
	{
		var c1 = new DoableStruct1() as IDoable;
		Assert.Equal(1, c1.Do());
		Assert.Equal(2, c1.Do());
		var clone = c1.ShallowClone();
		Assert.Equal(3, c1.Do());
		Assert.Equal(3, clone.Do());
	}

	[Fact]
	public void Struct_As_Interface_Should_Be_Cloned_For_DeepClone_Too()
	{
		var c1 = new DoableStruct1() as IDoable;
		Assert.Equal(1, c1.Do());
		Assert.Equal(2, c1.Do());
		var clone = c1.DeepClone();
		Assert.Equal(3, c1.Do());
		Assert.Equal(3, clone.Do());
	}

	[Fact]
	public void Struct_As_Interface_Should_Be_Cloned_In_Object()
	{
		var c1 = new DoableStruct1() as IDoable;
		var t = new Tuple<IDoable>(c1);
		Assert.Equal(1, t.Item1.Do());
		Assert.Equal(2, t.Item1.Do());
		var clone = t.ShallowClone();
		Assert.Equal(3, t.Item1.Do());
		// shallow clone do not copy object
		Assert.Equal(4, clone.Item1.Do());
	}

	[Fact]
	public void Struct_As_Interface_Should_Be_Cloned_For_DeepClone_Too_In_Object()
	{
		var c1 = new DoableStruct1() as IDoable;
		var t = new Tuple<IDoable>(c1);
		Assert.Equal(1, t.Item1.Do());
		Assert.Equal(2, t.Item1.Do());
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
