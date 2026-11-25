#pragma warning disable S1104, S2357 // Public fields are deliberate: these fixtures exist to test field cloning
#nullable disable

using Xunit;
using PanoramicData.DeepCloner.Test.Objects;

namespace PanoramicData.DeepCloner.Test;

public partial class SimpleObjectSpec
{
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
}
