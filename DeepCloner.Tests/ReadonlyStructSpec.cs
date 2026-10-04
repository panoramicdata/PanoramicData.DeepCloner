#nullable disable

using Xunit;

namespace PanoramicData.DeepCloner.Test;

/// <summary>
/// A struct whose fields are all readonly (a <c>readonly struct</c> or <c>readonly record struct</c>) must be
/// deep cloned like any other struct: its reference-type fields must not be shared with the source.
/// </summary>
public class ReadonlyStructSpec() : BaseTest(true)
{
	public readonly record struct Holder(int[] Items);

	public sealed class Box
	{
		public int Value { get; set; }
	}

	public readonly struct BoxHolder(Box box)
	{
		public Box Box { get; } = box;
	}

	public struct MutableHolder
	{
		public int[] Items { get; set; }
	}

	public sealed class HoldsReadonlyStruct
	{
		public Holder Holder { get; set; }
	}

	[Fact]
	public void Readonly_Struct_With_Array_Should_Be_Deep_Cloned()
	{
		var source = new Holder([1, 2, 3]);

		var clone = source.DeepClone();

		Assert.Equal([1, 2, 3], clone.Items);
		Assert.NotSame(source.Items, clone.Items);
	}

	[Fact]
	public void Readonly_Struct_With_Class_Should_Be_Deep_Cloned()
	{
		var source = new BoxHolder(new Box { Value = 7 });

		var clone = source.DeepClone();

		Assert.Equal(7, clone.Box.Value);
		Assert.NotSame(source.Box, clone.Box);
	}

	[Fact]
	public void Readonly_Struct_Inside_Class_Should_Be_Deep_Cloned()
	{
		var source = new HoldsReadonlyStruct { Holder = new Holder([4, 5]) };

		var clone = source.DeepClone();

		Assert.Equal([4, 5], clone.Holder.Items);
		Assert.NotSame(source.Holder.Items, clone.Holder.Items);
	}

	[Fact]
	public void One_Dim_Array_Of_Readonly_Structs_Should_Be_Deep_Cloned()
	{
		var source = new[] { new Holder([1]), new Holder([2]) };

		var clone = source.DeepClone();

		Assert.Equal([2], clone[1].Items);
		Assert.NotSame(source[1].Items, clone[1].Items);
	}

	[Fact]
	public void Two_Dim_Array_Of_Readonly_Structs_Should_Be_Deep_Cloned()
	{
		var source = new[,] { { new Holder([1]), new Holder([2]) }, { new Holder([3]), new Holder([4]) } };

		var clone = source.DeepClone();

		Assert.Equal([4], clone[1, 1].Items);
		Assert.NotSame(source[1, 1].Items, clone[1, 1].Items);
	}

	[Fact]
	public void Deep_Clone_To_Existing_Array_Of_Readonly_Structs_Should_Be_Deep()
	{
		var source = new[,] { { new Holder([1]), new Holder([2]) }, { new Holder([3]), new Holder([4]) } };
		var target = new Holder[2, 2];

		source.DeepCloneTo(target);

		Assert.Equal([4], target[1, 1].Items);
		Assert.NotSame(source[1, 1].Items, target[1, 1].Items);
	}

	[Fact]
	public void Shallow_Clone_Of_Readonly_Struct_Should_Share_References()
	{
		var source = new Holder([1, 2, 3]);

		var clone = source.ShallowClone();

		Assert.Same(source.Items, clone.Items);
	}

	[Fact]
	public void Mutable_Struct_With_Array_Should_Still_Be_Deep_Cloned()
	{
		var source = new MutableHolder { Items = [1, 2, 3] };

		var clone = source.DeepClone();

		Assert.Equal([1, 2, 3], clone.Items);
		Assert.NotSame(source.Items, clone.Items);
	}
}
