#pragma warning disable S1104 // Public fields are deliberate: these fixtures exist to test field cloning
#nullable disable

using Xunit;

namespace PanoramicData.DeepCloner.Test;

public partial class ArraysSpec
{
	public class C1(int x)
	{
		public int X { get; set; } = x;
	}

	[Fact]
	public void ClassArray_Should_Be_Cloned()
	{
		var arr = new[] { new C1(1), new C1(2) };
		var cloned = arr.DeepClone();
		Assert.Equal([1, 2], cloned.Select(c => c.X));
		Assert.All(arr.Zip(cloned), pair => Assert.NotEqual(pair.First, pair.Second));
	}


	public record struct S1(int X);

	public record struct S2
	{
		public C1 C;
	}

	[Fact]
	public void StructArray_Should_Be_Cloned()
		=> Assert.Equal([1, 2], new[] { new S1(1), new S1(2) }.DeepClone().Select(s => s.X));

	[Fact]
	public void StructArray_With_Class_Should_Be_Cloned()
	{
		var arr = new[] { new S2 { C = new C1(1) }, new S2 { C = new C1(2) } };
		var cloned = arr.DeepClone();
		Assert.Equal([1, 2], cloned.Select(s => s.C.X));
		Assert.All(arr.Zip(cloned), pair => Assert.NotEqual(pair.First.C, pair.Second.C));
	}

	[Fact]
	public void NullArray_Should_Be_Cloned()
	{
		var cloned = new C1[] { null, null }.DeepClone();
		Assert.Equal(2, cloned.Length);
		Assert.All(cloned, Assert.Null);
	}

	[Fact]
	public void NullAsArray_hould_Be_Cloned()
	{
		var arr = (int[])null;
		Assert.Null(arr.DeepClone());
	}
}
