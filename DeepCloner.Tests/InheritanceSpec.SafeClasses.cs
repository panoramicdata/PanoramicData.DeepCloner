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

	private static void AssertSafeFieldIsCloned(Safe1 safe, Func<Safe1, Safe1> cloneThroughHolder, Type expectedType)
	{
		var cloned = cloneThroughHolder(safe);
		Assert.False(safe == cloned);
		Assert.Equal(expectedType, cloned.GetType());
	}

	private static Safe1 ThroughV1(Safe1 safe) => new V1 { Safe = safe }.DeepClone().Safe;

	private static Safe1 ThroughV2(Safe1 safe) => new V2("X") { Safe = safe }.DeepClone().Safe;

	// these tests are overlapped by others, but for future can be helpful
	[Fact]
	public void Class_With_Safe_Class_Should_Be_Cloned()
		=> AssertSafeFieldIsCloned(new Safe1(), ThroughV1, typeof(Safe1));

	[Fact]
	public void Class_With_Safe_Class_Should_Be_Cloned_No_Default_Constructor()
		=> AssertSafeFieldIsCloned(new Safe1(), ThroughV2, typeof(Safe1));

	[Fact]
	public void Class_With_UnSafe_Class_Should_Be_Cloned()
		=> AssertSafeFieldIsCloned(new Unsafe1(), ThroughV1, typeof(Unsafe1));

	[Fact]
	public void Class_With_UnSafe_Class_Should_Be_Cloned_No_Default_Constructor()
		=> AssertSafeFieldIsCloned(new Unsafe1(), ThroughV2, typeof(Unsafe1));
}