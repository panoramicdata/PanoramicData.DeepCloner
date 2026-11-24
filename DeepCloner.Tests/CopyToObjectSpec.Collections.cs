#nullable disable

using Xunit;
using PanoramicData.DeepCloner;

namespace PanoramicData.DeepCloner.Test;

public partial class CopyToObjectSpec
{
	[Fact]
	public void Dictionary_Should_Be_Deeply_Cloned()
	{
		var d1 = new Dictionary<string, string> { { "A", "B" }, { "C", "D" } };
		var d2 = new Dictionary<string, string>();
		d1.DeepCloneTo(d2);
		d1["A"] = "E";
		Assert.Equal(2, d2.Count);
		Assert.Equal("B", d2["A"]);
		Assert.Equal("D", d2["C"]);

		// big dictionary
		d1.Clear();
		for (var i = 0; i < 1000; i++)
		{
			d1[i.ToString()] = i.ToString();
		}
		d1.DeepCloneTo(d2);
		Assert.Equal(1000, d2.Count);
		Assert.Equal("557", d2["557"]);
	}

	public class D1
	{
		public int A { get; set; }
	}

	public class D2 : D1
	{
		public int B { get; set; }

		public D2(D1 d1)
		{
			B = 14;
			d1.DeepCloneTo(this);
		}
	}

	[Fact]
	public void Inner_Implementation_In_Class_Should_Work()
	{
		var baseObject = new D1 { A = 12 };
		var wrapper = new D2(baseObject);
		Assert.Equal(12, wrapper.A);
		Assert.Equal(14, wrapper.B);
	}
}
