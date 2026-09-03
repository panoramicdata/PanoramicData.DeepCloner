#nullable disable

using Xunit;
using PanoramicData.DeepCloner;

namespace PanoramicData.DeepCloner.Test;

public class LoopCheckSpec() : BaseTest(true)
{
	public class C1
	{
		public int F { get; set; }

		public C1 A { get; set; }
	}

	[Fact]
	public void SimpleLoop_Should_Be_Handled()
	{
		var c1 = new C1();
		var c2 = new C1();
		c1.F = 1;
		c2.F = 2;
		c1.A = c2;
		c1.A.A = c1;
		var cloned = c1.DeepClone();

		Assert.NotNull(cloned.A);
		Assert.Equal(cloned.F, cloned.A.A.F);
		Assert.Equal(cloned, cloned.A.A);
	}

	[Fact]
	public void Object_Own_Loop_Should_Be_Handled()
	{
		var c1 = new C1
		{
			F = 1
		};
		c1.A = c1;
		var cloned = c1.DeepClone();

		Assert.NotNull(cloned.A);
		Assert.Equal(cloned.F, cloned.A.F);
		Assert.Equal(cloned, cloned.A);
	}

	[Fact]
	public void Array_Of_Same_Objects_Should_Be_Cloned()
	{
		var c1 = new C1();
		var arr = new[] { c1, c1, c1 };
		c1.F = 1;
		var cloned = arr.DeepClone();

		Assert.Equal(3, cloned.Length);
		Assert.Equal(cloned[1], cloned[0]);
		Assert.Equal(cloned[2], cloned[1]);
	}
}
