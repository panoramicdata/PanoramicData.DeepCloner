#pragma warning disable S1104 // Public fields are deliberate: these fixtures exist to test field cloning
#nullable disable

using Xunit;

namespace PanoramicData.DeepCloner.Test;

public partial class ArraysSpec
{
	[Fact]
	public void Issue_17_Spec()
	{
		// Deliberately asserting through each set's own Contains: it honours the set's
		// comparer, which is what this regression test is about. Assert.Contains would
		// use the default comparer instead, so xUnit2017 is suppressed here.
#pragma warning disable xUnit2017
		var set = new HashSet<string> { "value" };
		Assert.True(set.Contains("value"));

		var cloned = set.DeepClone();
		Assert.True(cloned.Contains("value"));

		var copyOfSet = new HashSet<string>(set, set.Comparer);
		Assert.True(copyOfSet.Contains("value"));

		var copyOfCloned = new HashSet<string>(cloned, cloned.Comparer);
		Assert.True(copyOfCloned.ToArray()[0] == "value");

		Assert.True(copyOfCloned.Contains("value"));
#pragma warning restore xUnit2017
	}

	[Fact]
	public void Check_Comparer_Cloning()
	{
		AssertComparerIsCloned<string>(true);

		AssertComparerIsCloned<int>(false);
		AssertComparerIsCloned<object>(false);
		AssertComparerIsCloned<FileShare>(false);
		AssertComparerIsCloned<byte[]>(false);
		AssertComparerIsCloned<byte>(false);
		AssertComparerIsCloned<int?>(false);
		AssertComparerIsCloned<HashSet<int>>(false);

		// each getter may return a new instance, so the two reads are deliberately separate
		Func<StringComparer>[] sameAfterClone = [() => StringComparer.Ordinal, () => StringComparer.InvariantCulture, () => StringComparer.InvariantCultureIgnoreCase];
		Func<StringComparer>[] differentAfterClone = [() => StringComparer.OrdinalIgnoreCase, () => StringComparer.CurrentCulture, () => StringComparer.CurrentCultureIgnoreCase];
		Assert.All(sameAfterClone, get => Assert.True(get() == get().DeepClone()));
		Assert.All(differentAfterClone, get => Assert.False(get() == get().DeepClone()));
	}

	private static void AssertComparerIsCloned<T>(bool expectedCloned)
	{
		var comparer = EqualityComparer<T>.Default;

		// checking by reference
		Assert.Equal(expectedCloned, comparer != comparer.DeepClone());
	}
}
