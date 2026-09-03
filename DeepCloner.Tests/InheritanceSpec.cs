#nullable disable

using Xunit;
using PanoramicData.DeepCloner;
using System;

namespace PanoramicData.DeepCloner.Test;

public class InheritanceSpec() : BaseTest(true)
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

	public struct S1 : IDisposable
	{
		public C1 X { get; set; }

		public int F;

		public void Dispose()
		{
		}
	}

	public struct S2 : IDisposable
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

	public struct S3
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

	[Fact]
	public void Descendant_In_Array_Should_Be_Cloned()
	{
		var c1 = new C1();
		var c2 = new C2();
		var arr = new[] { c1, c2 };

		var cloned = arr.DeepClone();
		Assert.IsType<C1>(cloned[0]);
		Assert.IsType<C2>(cloned[1]);
	}

	[Fact]
	public void Struct_Casted_To_Interface_Should_Be_Cloned()
	{
		var s1 = new S1
		{
			F = 1
		};
		var disp = s1 as IDisposable;
		var cloned = disp.DeepClone();
		s1.F = 2;
		Assert.IsType<S1>(cloned);
		Assert.Equal(1, ((S1)cloned).F);
	}

	public IDisposable CCC(IDisposable xx)
	{
		var x = (S1)xx;
		return x;
	}

	[Fact]
	public void Class_Casted_To_Object_Should_Be_Cloned()
	{
		var c3 = new C3
		{
			X = new C1()
		};
		var obj = c3 as object;
		var cloned = obj.DeepClone();
		Assert.IsType<C3>(cloned);
		Assert.NotEqual(cloned, c3);
		Assert.NotNull(((C3)cloned).X);
		Assert.NotEqual(c3.X, ((C3)cloned).X);
	}

	[Fact]
	public void Class_Casted_To_Interface_Should_Be_Cloned()
	{
		var c1 = new C1();
		var disp = c1 as IDisposable;
		var cloned = disp.DeepClone();
		Assert.NotEqual(cloned, c1);
		Assert.IsType<C1>(cloned);
	}

	[Fact]
	public void Struct_Casted_To_Interface_With_Class_As_Interface_Should_Be_Cloned()
	{
		var s2 = new S2
		{
			X = new C1()
		};
		var disp = s2 as IDisposable;
		var cloned = disp.DeepClone();
		Assert.IsType<S2>(cloned);
		Assert.IsType<C1>(((S2)cloned).X);
		Assert.NotEqual(s2.X, ((S2)cloned).X);
	}

	[Fact]
	public void Array_Of_Struct_Casted_To_Interface_Should_Be_Cloned()
	{
		var s1 = new S1();
		var arr = new IDisposable[] { s1, s1 };
		var clonedArr = arr.DeepClone();
		Assert.Equal(clonedArr[1], clonedArr[0]);
	}

	public class Safe1
	{
	}

	public class Safe2
	{
	}

	public class Unsafe1 : Safe1
	{
		public object X;
	}

	public class V1
	{
		public Safe1 Safe;
	}

    public class V2
	{
      public V2(string x)
		{
			_ = x;
		}

		public Safe1 Safe;
	}

	// these tests are overlapped by others, but for future can be helpful
	[Fact]
	public void Class_With_Safe_Class_Should_Be_Cloned()
	{
		var v = new V1
		{
			Safe = new Safe1()
		};
		var v2 = v.DeepClone();
		Assert.False(v.Safe == v2.Safe);
	}

	[Fact]
	public void Class_With_Safe_Class_Should_Be_Cloned_No_Default_Constructor()
	{
		var v = new V2("X")
		{
			Safe = new Safe1()
		};
		var v2 = v.DeepClone();
		Assert.False(v.Safe == v2.Safe);
	}

	[Fact]
	public void Class_With_UnSafe_Class_Should_Be_Cloned()
	{
		var v = new V1
		{
			Safe = new Unsafe1()
		};
		var v2 = v.DeepClone();
		Assert.False(v.Safe == v2.Safe);
		Assert.Equal(typeof(Unsafe1), v2.Safe.GetType());
	}

	[Fact]
	public void Class_With_UnSafe_Class_Should_Be_Cloned_No_Default_Constructor()
	{
		var v = new V2("X")
		{
			Safe = new Unsafe1()
		};
		var v2 = v.DeepClone();
		Assert.False(v.Safe == v2.Safe);
		Assert.Equal(typeof(Unsafe1), v2.Safe.GetType());
	}
}
