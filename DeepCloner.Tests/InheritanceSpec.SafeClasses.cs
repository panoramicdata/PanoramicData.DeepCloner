#pragma warning disable S1104, S2357 // Public fields are deliberate: these fixtures exist to test field cloning
#nullable disable

using Xunit;
using PanoramicData.DeepCloner;

namespace PanoramicData.DeepCloner.Test;

public partial class InheritanceSpec
{
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
