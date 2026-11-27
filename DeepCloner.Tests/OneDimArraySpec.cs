#nullable disable

using Xunit;
using PanoramicData.DeepCloner;

namespace PanoramicData.DeepCloner.Test;

/// <summary>
/// Characterisation tests for the one-dimensional array clone paths: the DeepClone path and the
/// DeepCloneTo / ShallowCloneTo path, each with safe, struct and class element types.
/// They pin the behaviour as it was before any refactoring of those paths.
/// </summary>
public class OneDimArraySpec() : BaseTest(true)
{
	public enum Colour
	{
		Red,
		Green
	}

	public class Node(int value)
	{
		public int Value { get; set; } = value;

		public Node Next { get; set; }
	}

	public record struct Plain(int X, Colour C);

	public record struct Holder
	{
		public Node Ref { get; set; }

		public int X { get; set; }
	}

	public class Animal(string name)
	{
		public string Name { get; } = name;
	}

	public class Dog(string name) : Animal(name);

	// A Dog[] seen as an Animal[]: the runtime element type is narrower than the static one.
	private static Animal[] CreateCovariantArray() => (Animal[])Array.CreateInstance(typeof(Dog), 2);

	private static T[] Copy<T>(T[] from, T[] to, bool isDeep)
	{
		if (isDeep)
		{
			from.DeepCloneTo(to);
		}
		else
		{
			from.ShallowCloneTo(to);
		}

		return to;
	}

	// ---- DeepClone: safe element types

	[Fact]
	public void DeepClone_Primitives_Are_Copied_Into_A_New_Array()
	{
		var source = new[] { 1, 2, 3 };
		var clone = source.DeepClone();
		Assert.NotSame(source, clone);
		Assert.Equal(source, clone);
		clone[0] = 99;
		Assert.Equal(1, source[0]);
	}

	[Fact]
	public void DeepClone_Empty_Array_Gives_A_New_Empty_Array()
	{
		var source = new int[0];
		var clone = source.DeepClone();
		Assert.NotSame(source, clone);
		Assert.Empty(clone);
	}

	[Fact]
	public void DeepClone_Strings_Keep_Their_References()
	{
		var source = new[] { "a", null, new string('b', 3) };
		var clone = source.DeepClone();
		Assert.NotSame(source, clone);
		Assert.Same(source[0], clone[0]);
		Assert.Null(clone[1]);
		Assert.Same(source[2], clone[2]);
	}

	[Fact]
	public void DeepClone_Enums_And_Plain_Structs_Are_Copied_By_Value()
	{
		var enums = new[] { Colour.Green, Colour.Red };
		Assert.Equal(enums, enums.DeepClone());

		var plains = new[] { new Plain(1, Colour.Green), new Plain(2, Colour.Red) };
		var clone = plains.DeepClone();
		Assert.NotSame(plains, clone);
		Assert.Equal(plains, clone);
	}

	// ---- DeepClone: struct elements with references

	[Fact]
	public void DeepClone_Structs_With_References_Get_Cloned_References()
	{
		var shared = new Node(5);
		var source = new[] { new Holder { Ref = shared, X = 1 }, new Holder { Ref = shared, X = 2 }, default };
		var clone = source.DeepClone();

		Assert.NotSame(source, clone);
		Assert.Equal([1, 2, 0], clone.Select(h => h.X));
		Assert.NotSame(shared, clone[0].Ref);
		Assert.Equal(5, clone[0].Ref.Value);
		Assert.Same(clone[0].Ref, clone[1].Ref);
		Assert.Null(clone[2].Ref);
	}

	// ---- DeepClone: class element types

	[Fact]
	public void DeepClone_Classes_Are_Cloned_And_Nulls_Stay_Null()
	{
		var source = new[] { new Node(1), null, new Node(3) };
		var clone = source.DeepClone();

		Assert.NotSame(source, clone);
		Assert.NotSame(source[0], clone[0]);
		Assert.Equal(1, clone[0].Value);
		Assert.Null(clone[1]);
		Assert.Equal(3, clone[2].Value);
	}

	[Fact]
	public void DeepClone_Shared_Class_References_Stay_Shared()
	{
		var shared = new Node(7);
		var clone = new[] { shared, shared, shared, shared, shared }.DeepClone();

		Assert.NotSame(shared, clone[0]);
		Assert.All(clone, item => Assert.Same(clone[0], item));
	}

	[Fact]
	public void DeepClone_Object_Cycles_Through_The_Elements_Are_Preserved()
	{
		var a = new Node(1);
		var b = new Node(2) { Next = a };
		a.Next = b;
		var clone = new[] { a, b }.DeepClone();

		Assert.Same(clone[1], clone[0].Next);
		Assert.Same(clone[0], clone[1].Next);
		Assert.NotSame(a, clone[0]);
	}

	[Fact]
	public void DeepClone_Array_Containing_Itself_Points_At_The_Clone()
	{
		var source = new object[2];
		source[0] = source;
		source[1] = "text";
		var clone = source.DeepClone();

		Assert.NotSame(source, clone);
		Assert.Same(clone, clone[0]);
		Assert.Equal("text", clone[1]);
	}

	[Fact]
	public void DeepClone_Covariant_Array_Keeps_Its_Runtime_Type()
	{
		var source = CreateCovariantArray();
		source[0] = new Dog("rex");
		var clone = source.DeepClone();

		Assert.IsType<Dog[]>(clone);
		Assert.IsType<Dog>(clone[0]);
		Assert.NotSame(source[0], clone[0]);
		Assert.Equal("rex", clone[0].Name);
		Assert.Null(clone[1]);
	}

	[Fact]
	public void DeepClone_Object_Array_With_Mixed_Elements()
	{
		var node = new Node(4);
		object[] source = [1, "two", node, Colour.Green, null];
		var clone = source.DeepClone();

		Assert.Equal(1, clone[0]);
		Assert.Same(source[1], clone[1]);
		Assert.NotSame(node, clone[2]);
		Assert.Equal(4, ((Node)clone[2]).Value);
		Assert.Equal(Colour.Green, clone[3]);
		Assert.Null(clone[4]);
	}

	[Fact]
	public void DeepClone_Jagged_Arrays_Are_Cloned_At_Every_Level()
	{
		var inner = new[] { 1, 2 };
		int[][] source = [inner, inner, null, [3]];
		var clone = source.DeepClone();

		Assert.NotSame(source, clone);
		Assert.NotSame(inner, clone[0]);
		Assert.Same(clone[0], clone[1]);
		Assert.Null(clone[2]);
		Assert.Equal([3], clone[3]);
	}

	// ---- CloneTo: safe element types

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void CloneTo_Primitives_Fill_The_Target_And_Return_It(bool isDeep)
	{
		var to = new[] { 9, 9, 9 };
		var result = Copy([1, 2, 3], to, isDeep);
		Assert.Same(to, result);
		Assert.Equal([1, 2, 3], to);
	}

	[Theory]
	[InlineData(false, 3)]
	[InlineData(true, 3)]
	[InlineData(false, 5)]
	[InlineData(true, 5)]
	public void CloneTo_Truncates_Or_Leaves_The_Rest_Of_The_Target(bool isDeep, int targetLength)
	{
		var to = Enumerable.Repeat(9, targetLength).ToArray();
		Copy([1, 2, 3, 4], to, isDeep);
		Assert.Equal(targetLength, to.Length);
		Assert.Equal(Enumerable.Range(1, 4).Take(targetLength).Concat(Enumerable.Repeat(9, Math.Max(0, targetLength - 4))), to);
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void CloneTo_Strings_Enums_And_Plain_Structs_Are_Copied_By_Value(bool isDeep)
	{
		var strings = Copy(["a", null], new string[3], isDeep);
		Assert.Equal(["a", null, null], strings);

		var enums = Copy([Colour.Green], new Colour[2], isDeep);
		Assert.Equal([Colour.Green, Colour.Red], enums);

		var plains = Copy([new Plain(1, Colour.Green)], new Plain[2], isDeep);
		Assert.Equal([new Plain(1, Colour.Green), default], plains);
	}

	// ---- CloneTo: class element types

	[Fact]
	public void ShallowCloneTo_Classes_Share_The_Element_References()
	{
		var first = new Node(1);
		var to = Copy([first, null], new Node[3], false);
		Assert.Same(first, to[0]);
		Assert.Null(to[1]);
		Assert.Null(to[2]);
	}

	[Fact]
	public void DeepCloneTo_Classes_Are_Cloned_And_Keep_Shared_References()
	{
		var shared = new Node(1);
		var to = Copy([shared, shared, null, new Node(4)], new Node[5], true);

		Assert.NotSame(shared, to[0]);
		Assert.Equal(1, to[0].Value);
		Assert.Same(to[0], to[1]);
		Assert.Null(to[2]);
		Assert.Equal(4, to[3].Value);
		Assert.Null(to[4]);
	}

	[Fact]
	public void DeepCloneTo_Class_Elements_Beyond_The_Source_Length_Are_Untouched()
	{
		var leftover = new Node(9);
		var to = Copy([new Node(1)], [null, leftover], true);
		Assert.Equal(1, to[0].Value);
		Assert.Same(leftover, to[1]);
	}

	[Fact]
	public void DeepCloneTo_Class_Elements_Are_Truncated_When_The_Target_Is_Shorter()
	{
		var to = Copy([new Node(1), new Node(2), new Node(3)], new Node[2], true);
		Assert.Equal([1, 2], to.Select(n => n.Value));
	}

	[Fact]
	public void DeepCloneTo_Array_Containing_Itself_Points_At_The_Target()
	{
		var from = new object[2];
		from[0] = from;
		from[1] = "text";
		var to = new object[2];
		from.DeepCloneTo(to);

		Assert.Same(to, to[0]);
		Assert.Equal("text", to[1]);
	}

	[Fact]
	public void DeepCloneTo_Object_Cycles_Through_The_Elements_Are_Preserved()
	{
		var a = new Node(1);
		var b = new Node(2) { Next = a };
		a.Next = b;
		var to = Copy([a, b], new Node[2], true);

		Assert.Same(to[1], to[0].Next);
		Assert.Same(to[0], to[1].Next);
	}

	[Fact]
	public void DeepCloneTo_Covariant_Array_Is_Filled_With_Cloned_Elements()
	{
		Animal[] from = [new Dog("rex"), null];
		var to = CreateCovariantArray();
		from.DeepCloneTo(to);

		Assert.IsType<Dog>(to[0]);
		Assert.NotSame(from[0], to[0]);
		Assert.Equal("rex", to[0].Name);
		Assert.Null(to[1]);
	}

	[Fact]
	public void DeepCloneTo_Jagged_Arrays_Clone_The_Inner_Arrays()
	{
		var inner = new[] { 1, 2 };
		var to = Copy([inner, inner, null], new int[3][], true);

		Assert.NotSame(inner, to[0]);
		Assert.Equal([1, 2], to[0]);
		Assert.Same(to[0], to[1]);
		Assert.Null(to[2]);
	}

	// ---- CloneTo: struct elements with references
	// The struct path rebuilds each target element from the TARGET element, not from the source element,
	// so the source references are never copied. This test pins that existing behaviour.

	[Fact]
	public void DeepCloneTo_Struct_Elements_Are_Rebuilt_From_The_Target_Elements()
	{
		var sourceRef = new Node(1);
		var targetRef = new Node(9);
		var from = new[] { new Holder { Ref = sourceRef, X = 1 } };
		var to = new[] { new Holder { Ref = targetRef, X = 5 } };
		from.DeepCloneTo(to);

		Assert.Equal(5, to[0].X);
		Assert.NotSame(targetRef, to[0].Ref);
		Assert.Equal(9, to[0].Ref.Value);
		Assert.NotSame(sourceRef, to[0].Ref);
	}

	[Fact]
	public void DeepCloneTo_Struct_Elements_Leave_Default_Targets_Default()
	{
		var from = new[] { new Holder { Ref = new Node(1), X = 1 }, new Holder { Ref = new Node(2), X = 2 } };
		var to = new Holder[3];
		from.DeepCloneTo(to);

		Assert.All(to, holder => Assert.Null(holder.Ref));
		Assert.All(to, holder => Assert.Equal(0, holder.X));
	}

	[Fact]
	public void ShallowCloneTo_Struct_Elements_Are_Copied_Whole()
	{
		var shared = new Node(1);
		var to = Copy([new Holder { Ref = shared, X = 1 }], new Holder[2], false);
		Assert.Same(shared, to[0].Ref);
		Assert.Equal(1, to[0].X);
		Assert.Null(to[1].Ref);
	}
}
