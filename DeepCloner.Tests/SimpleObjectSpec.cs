#nullable disable

using Xunit;
using PanoramicData.DeepCloner.Test.Objects;
using System;

namespace PanoramicData.DeepCloner.Test;

public class SimpleObjectSpec() : BaseTest(true)
{
	[Fact]
	public void SimpleObject_Should_Be_Cloned()
	{
		var obj = new TestObject1 { Int = 42, Byte = 42, Short = 42, Long = 42, DateTime = new DateTime(2001, 01, 01), Char = 'X', Decimal = 1.2m, Double = 1.3, Float = 1.4f, String = "test1", UInt = 42, ULong = 42, UShort = 42, Bool = true, IntPtr = new nint(42), UIntPtr = new nuint(42), Enum = AttributeTargets.Delegate };

		var cloned = obj.DeepClone();
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

	public struct S1
	{
		public int A;
	}

	public struct S2
	{
		public S3 S;
	}

	public struct S3
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

	public struct S4
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

	[Fact]
	public void Privitive_Should_Be_Cloned()
	{
		Assert.Equal(3, 3.DeepClone());
		Assert.Equal('x', 'x'.DeepClone());
		Assert.Equal("xxxxxxxxxx yyyyyyyyyyyyyy", "xxxxxxxxxx yyyyyyyyyyyyyy".DeepClone());
		Assert.Equal(string.Empty, string.Empty.DeepClone());
		Assert.True(ReferenceEquals("y".DeepClone(), "y"));
		Assert.Equal(DateTime.MinValue, DateTime.MinValue.DeepClone());
		Assert.Equal(AttributeTargets.Delegate, AttributeTargets.Delegate.DeepClone());
		Assert.Null(((object)null).DeepClone());
		var obj = new object();
		Assert.NotNull(obj.DeepClone());
		Assert.True(true.DeepClone());
		Assert.True((bool)((object)true).DeepClone());
		Assert.Equal(typeof(object), obj.DeepClone().GetType());
		Assert.NotEqual(obj, obj.DeepClone());
	}

	[Fact]
	public void Guid_Should_Be_Cloned()
	{
		var g = Guid.NewGuid();
		Assert.Equal(g, g.DeepClone());
	}

	private class UnsafeObject
	{
		public unsafe void* Void;
		public unsafe int* Int;
	}

	[Fact]
	public void Unsafe_Should_Be_Cloned()
	{
		var u = new UnsafeObject();
		var i = 1;
		var j = 2;
		unsafe
		{
			u.Int = &i;
			u.Void = &i;
		}

		var cloned = u.DeepClone();
		unsafe
		{
			u.Int = &j;
			Assert.True(cloned.Int == &i);
			Assert.True(cloned.Void == &i);
		}
	}

	[Fact]
	public void String_In_Class_Should_Not_Be_Cloned()
	{
		var c = new C3 { X = "aaa" };
		var cloned = c.DeepClone();
		Assert.Equal(c.X, cloned.X);
		Assert.True(ReferenceEquals(cloned.X, c.X));
	}

	public sealed class C6
	{
		public readonly int X = 1;

		private readonly object y = new();

        private readonly StructWithObject z = default;

		public object GetY()
		{
			return y;
		}

       public object GetZ()
		{
			return z.Z;
		}
	}

	public struct StructWithObject
	{
		public readonly object Z;
	}

	[Fact]
	public void Object_With_Readonly_Fields_Should_Be_Cloned()
	{
		var c = new C6();
		var clone = c.DeepClone();
		Assert.NotEqual(c, clone);
		Assert.Equal(1, clone.X);
		Assert.NotNull(clone.GetY());
		Assert.NotEqual(c.GetY(), clone.GetY());
		Assert.NotEqual(c.GetY(), clone.GetY());
	}

	public class VirtualClass1
	{
		public virtual int A { get; set; }

		public virtual int B { get; set; }

		// not safe
		public object X { get; set; }
	}

	public class VirtualClass2 : VirtualClass1
	{
		public override int B { get; set; }
	}

	// Nothings special, just for checking
	[Fact]
	public void Class_With_Virtual_Methods_Should_Be_Cloned()
	{
		var v2 = new VirtualClass2
		{
			A = 1,
			B = 2
		};
		var v1 = v2 as VirtualClass1;
		v1.A = 3;
		var clone = v1.DeepClone() as VirtualClass2;
		v2.B = 0;
		v2.A = 0;
		Assert.Equal(2, clone.B);
		Assert.Equal(3, clone.A);
	}

	// DBNull is compared by value, so, we don't need to clone it
	[Fact]
	public void DbNull_Should_Not_Be_Cloned()
	{
		var v = DBNull.Value;
		Assert.True(v == v.DeepClone());
		Assert.True(v == v.ShallowClone());
	}

	public class EmptyClass { }

	// Empty class does not have any mutable properties, so, it safe to use same class in cloning
	[Fact(Skip = "Think about logic, which is better to clone or not to clone, I do not know, but it changes current logic seriously")]
	// e.g. new object() frequently use for locks - if we leave same object - we'll receive same lock in different classes
	// todo: think about another reasons
	public void Empty_Should_Not_Be_Cloned()
	{
		var v = new EmptyClass();
		Assert.True(ReferenceEquals(v, v.DeepClone()));
		Assert.True(ReferenceEquals(v, v.ShallowClone()));
	}

	// Reflection classes should not be cloned
	[Fact]
	public void MethodInfo_Should_Not_Be_Cloned()
	{
#if NETCORE13
		var v = GetType().GetTypeInfo().GetMethod("MethodInfo_Should_Not_Be_Cloned");
#else
		var v = GetType().GetMethod("MethodInfo_Should_Not_Be_Cloned");
#endif
		Assert.True(ReferenceEquals(v, v.DeepClone()));
		Assert.True(ReferenceEquals(v, v.ShallowClone()));
	}

	public class Readonly1(string x)
	{
		public readonly object X = x;

		public object Z = new();
	}

	[Fact]
	public void Readonly_Field_Should_Remain_ReadOnly()
	{
		var c = new Readonly1("Z").DeepClone();
		Assert.Equal("Z", c.X);
		Assert.True(typeof(Readonly1).GetField("X").IsInitOnly);
	}

	[Fact]
	public void System_Type_Should_Not_Be_Cloned()
	{
		// it used for dictionaries as key. there are no sense to copy it
		var t = GetType(); // RuntimeType
		var clone = t.DeepClone();
		Assert.True(ReferenceEquals(t, clone));
	}
}
