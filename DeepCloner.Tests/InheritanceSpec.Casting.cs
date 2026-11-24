#pragma warning disable S1104, S2357 // Public fields are deliberate: these fixtures exist to test field cloning
#nullable disable

using Xunit;
using PanoramicData.DeepCloner;

namespace PanoramicData.DeepCloner.Test;

public partial class InheritanceSpec
{
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
}
