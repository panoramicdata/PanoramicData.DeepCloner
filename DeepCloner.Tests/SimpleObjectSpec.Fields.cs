#pragma warning disable S1104, S2357 // Public fields are deliberate: these fixtures exist to test field cloning
#nullable disable

using Xunit;
using PanoramicData.DeepCloner.Test.Objects;

namespace PanoramicData.DeepCloner.Test;

public partial class SimpleObjectSpec
{
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

	public record struct StructWithObject
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
	// Explicit (keep, but do not run by default): an open upstream design question, and returning the same instance for a field-less object (e.g. new object() used as a lock) would make clones share a lock.
	[Fact(Explicit = true)]
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
