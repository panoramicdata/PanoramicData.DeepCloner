#nullable disable

using Xunit;

namespace PanoramicData.DeepCloner.Test.Objects;

internal static class TestObject1Sample
{
	public static TestObject1 Create() => new()
	{
		Int = 42,
		Byte = 42,
		Short = 42,
		Long = 42,
		DateTime = new DateTime(2001, 01, 01),
		Char = 'X',
		Decimal = 1.2m,
		Double = 1.3,
		Float = 1.4f,
		String = "test1",
		UInt = 42,
		ULong = 42,
		UShort = 42,
		Bool = true,
		IntPtr = new nint(42),
		UIntPtr = new nuint(42),
		Enum = AttributeTargets.Delegate
	};

	public static void AssertMatches(TestObject1 cloned)
	{
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
}