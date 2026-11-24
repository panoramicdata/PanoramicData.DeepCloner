#pragma warning disable S1104, S2357 // Public fields are deliberate: these fixtures exist to test field cloning
#nullable disable

using Xunit;
using PanoramicData.DeepCloner;
using System;

namespace PanoramicData.DeepCloner.Test;

public partial class InheritanceSpec() : BaseTest(true)
{
	public class C1 : IDisposable
	{
		public int X;

		public int Y;

		public object O; // make it not safe

		public void Dispose()
		{
		}
	}

	public class C2 : C1
	{
		public new int X;

		public int Z;
	}

	public class C1P : IDisposable
	{
		public int X { get; set; }

		public int Y { get; set; }

		public object O; // make it not safe

		public void Dispose()
		{
		}
	}

	public class C2P : C1P
	{
		public new int X { get; set; }

		public int Z { get; set; }
	}

	public record struct S1 : IDisposable
	{
		public C1 X { get; set; }

		public int F;

		public void Dispose()
		{
		}
	}

	public record struct S2 : IDisposable
	{
		public IDisposable X { get; set; }

		public void Dispose()
		{
		}
	}

	public class C3
	{
		public C1 X { get; set; }
	}

	[Fact]
	public void Descendant_Should_Be_Cloned()
	{
		var c2 = new C2
		{
			X = 1,
			Y = 2,
			Z = 3
		};
		var c1 = c2 as C1;
		c1.X = 4;
		var cloned = c1.DeepClone();
		Assert.IsType<C2>(cloned);
		Assert.Equal(4, cloned.X);
		Assert.Equal(2, cloned.Y);
		Assert.Equal(3, ((C2)cloned).Z);
		Assert.Equal(1, ((C2)cloned).X);
	}

	[Fact]
	public void Class_Should_Be_Cloned_With_Parents()
	{
		var c2 = new C2P
		{
			X = 1,
			Y = 2,
			Z = 3
		};
		var c1 = c2 as C1P;
		c1.X = 4;
		var cloned = c2.DeepClone();
		c2.X = 100;
		c2.Y = 100;
		c2.Z = 100;
		c1.X = 100;
		Assert.IsType<C2P>(cloned);
		Assert.Equal(4, ((C1P)cloned).X);
		Assert.Equal(2, cloned.Y);
		Assert.Equal(3, cloned.Z);
		Assert.Equal(1, cloned.X);
	}

	public record struct S3
	{
		public C1P X { get; set; }

		public C1P Y { get; set; }
	}

	[Fact]
	public void Struct_Should_Be_Cloned_With_Class_With_Parents()
	{
		var c2 = new S3
		{
			X = new C1P(),
			Y = new C2P()
		};

		c2.X.X = 1;
		c2.X.Y = 2;
		c2.Y.X = 3;
		c2.Y.Y = 4;
		((C2P)c2.Y).X = 5;
		((C2P)c2.Y).Z = 6;
		var cloned = c2.DeepClone();
		c2.X.X = 100;
		c2.X.Y = 200;
		c2.Y.X = 300;
		c2.Y.Y = 400;
		((C2P)c2.Y).X = 500;
		((C2P)c2.Y).Z = 600;
		Assert.IsType<S3>(cloned);
		Assert.Equal(1, cloned.X.X);
		Assert.Equal(2, cloned.X.Y);
		Assert.Equal(3, cloned.Y.X);
		Assert.Equal(4, cloned.Y.Y);
		Assert.Equal(5, ((C2P)cloned.Y).X);
		Assert.Equal(6, ((C2P)cloned.Y).Z);
	}
}
