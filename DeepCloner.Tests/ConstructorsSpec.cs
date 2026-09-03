#nullable disable

using Xunit;
using PanoramicData.DeepCloner;
using System;

namespace PanoramicData.DeepCloner.Test;

public class ConstructorsSpec() : BaseTest(true)
{
	public class T1
	{
		private T1()
		{
		}

		public static T1 Create()
		{
			return new T1();
		}

		public int X { get; set; }
	}

 public class T2
	{
      public T2(int arg1, int arg2)
		{
			_ = arg1;
			_ = arg2;
		}

		public int X { get; set; }
	}

	public class ExClass
	{
		public ExClass()
		{
			throw new Exception();
		}

		public ExClass(string x)
		{
			// does not throw here
		}

		public override bool Equals(object obj)
		{
			throw new Exception();
		}

		public override int GetHashCode()
		{
			throw new Exception();
		}

		public override string ToString()
		{
			throw new Exception();
		}
	}

#if !NETCORE
	public class ClonableClass : ICloneable
	{
		public object X { get; set; }

		public object Clone()
		{
			throw new NotImplementedException();
		}
	}

	[Fact]
	public void Cloner_Should_Not_Call_Any_Method_Of_Clonable_Class()
	{
		// just for check, ensure no hidden behaviour in MemberwiseClone
		_ = new ClonableClass().DeepClone();
		_ = new { X = new ClonableClass() }.DeepClone();
	}
#endif

	[Fact]
	public void Object_With_Private_Constructor_Should_Be_Cloned()
	{
		var t1 = T1.Create();
		t1.X = 42;
		var cloned = t1.DeepClone();
		t1.X = 0;
		Assert.Equal(42, cloned.X);
	}

	[Fact]
	public void Object_With_Complex_Constructor_Should_Be_Cloned()
	{
		var t2 = new T2(1, 2)
		{
			X = 42
		};
		var cloned = t2.DeepClone();
		t2.X = 0;
		Assert.Equal(42, cloned.X);
	}

	[Fact]
	public void Anonymous_Object_Should_Be_Cloned()
	{
		var t2 = new { A = 1, B = "x" };
		var cloned = t2.DeepClone();
		Assert.Equal(1, cloned.A);
		Assert.Equal("x", cloned.B);
	}

#if !NETCORE
	private class C3 : ContextBoundObject
	{
	}

	private class C4 : MarshalByRefObject
	{
	}

	[Fact]
	public void ContextBound_Object_Should_Be_Cloned()
	{
		// FormatterServices.CreateUninitializedObject cannot use context-bound objects
		var c = new C3();
		var cloned = c.DeepClone();
		Assert.NotNull(cloned);
	}

	[Fact]
	public void MarshalByRef_Object_Should_Be_Cloned()
	{
		// FormatterServices.CreateUninitializedObject cannot use context-bound objects
		var c = new C4();
		var cloned = c.DeepClone();
		Assert.NotNull(cloned);
	}
#endif

	[Fact]
	public void Cloner_Should_Not_Call_Any_Method_Of_Class_Be_Cloned()
	{
		_ = new ExClass("x").DeepClone();
		var exClass = new ExClass("x");
		_ = new[] { exClass, exClass }.DeepClone();
	}
}
