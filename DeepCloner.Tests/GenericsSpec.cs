#nullable disable

using Xunit;
using PanoramicData.DeepCloner;
using System;

namespace PanoramicData.DeepCloner.Test;

public class GenericsSpec() : BaseTest(true)
{
	[Fact]
	public void Tuple_Should_Be_Cloned()
	{
		var c = new Tuple<int, int>(1, 2).DeepClone();
		Assert.Equal(1, c.Item1);
		Assert.Equal(2, c.Item2);

		c = new Tuple<int, int>(1, 2).ShallowClone();
		Assert.Equal(1, c.Item1);
		Assert.Equal(2, c.Item2);

		var cc = new Tuple<int, int, int, int, int, int, int>(1, 2, 3, 4, 5, 6, 7).DeepClone();
		Assert.Equal(7, cc.Item7);

		var tuple = new Tuple<int, Generic<object>>(1, new Generic<object>());
		tuple.Item2.Value = tuple;
		var ccc = tuple.DeepClone();
		Assert.Equal(ccc.Item2.Value, ccc);
	}

	[Fact]
	public void Generic_Should_Be_Cloned()
	{
		var c = new Generic<int>
		{
			Value = 12
		};
		Assert.Equal(12, c.DeepClone().Value);

		var c2 = new Generic<object>
		{
			Value = 12
		};
		Assert.Equal(12, c2.DeepClone().Value);
	}

	public class C1
	{
		public int X { get; set; }
	}

	public class C2 : C1
	{
		public int Y { get; set; }
	}

	public class Generic<T>
	{
		public T Value { get; set; }
	}

	[Fact]
	public void Tuple_Should_Be_Cloned_With_Inheritance_And_Same_Object()
	{
		var c2 = new C2 { X = 1, Y = 2 };
		var c = new Tuple<C1, C2>(c2, c2).DeepClone();
		var cs = new Tuple<C1, C2>(c2, c2).ShallowClone();
		c2.X = 42;
		c2.Y = 42;
		Assert.Equal(1, c.Item1.X);
		Assert.Equal(2, c.Item2.Y);
		Assert.Equal(c.Item1, c.Item2);

		Assert.Equal(42, cs.Item1.X);
		Assert.Equal(42, cs.Item2.Y);
		Assert.Equal(cs.Item1, cs.Item2);
	}
}
