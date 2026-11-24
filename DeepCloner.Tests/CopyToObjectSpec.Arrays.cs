#nullable disable

using Xunit;
using PanoramicData.DeepCloner;

namespace PanoramicData.DeepCloner.Test;

public partial class CopyToObjectSpec
{
	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void Array_Should_Be_Cloned_Correct_Size(bool isDeep)
	{
		var arrFrom = new[] { 1, 2, 3 };
		var arrTo = new[] { 4, 5, 6 };
		if (isDeep)
		{
			arrFrom.DeepCloneTo(arrTo);
		}
		else
		{
			arrFrom.ShallowCloneTo(arrTo);
		}
		Assert.Equal(3, arrTo.Length);
		Assert.Equal(1, arrTo[0]);
		Assert.Equal(2, arrTo[1]);
		Assert.Equal(3, arrTo[2]);
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void Array_Should_Be_Cloned_From_Is_Bigger(bool isDeep)
	{
		var arrFrom = new[] { 1, 2, 3 };
		var arrTo = new[] { 4, 5 };
		if (isDeep)
		{
			arrFrom.DeepCloneTo(arrTo);
		}
		else
		{
			arrFrom.ShallowCloneTo(arrTo);
		}
		Assert.Equal(2, arrTo.Length);
		Assert.Equal(1, arrTo[0]);
		Assert.Equal(2, arrTo[1]);
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void Array_Should_Be_Cloned_From_Is_Smaller(bool isDeep)
	{
		var arrFrom = new[] { 1, 2 };
		var arrTo = new[] { 4, 5, 6 };
		if (isDeep)
		{
			arrFrom.DeepCloneTo(arrTo);
		}
		else
		{
			arrFrom.ShallowCloneTo(arrTo);
		}
		Assert.Equal(3, arrTo.Length);
		Assert.Equal(1, arrTo[0]);
		Assert.Equal(2, arrTo[1]);
		Assert.Equal(6, arrTo[2]);
	}

	[Fact]
	public void Shallow_Array_Should_Be_Cloned()
	{
		var c1 = new C1();
		var arrFrom = new[] { c1, c1, c1 };
		var arrTo = new C1[4];
		arrFrom.ShallowCloneTo(arrTo);
		Assert.Equal(4, arrTo.Length);
		Assert.Equal(c1, arrTo[0]);
		Assert.Equal(c1, arrTo[1]);
		Assert.Equal(c1, arrTo[2]);
		Assert.Null(arrTo[3]);
	}

	[Fact]
	public void Deep_Array_Should_Be_Cloned()
	{
		var c1 = new C4();
		var c3 = new C3 { A = c1, B = c1 };
		var arrFrom = new[] { c3, c3, c3 };
		var arrTo = new C3[4];
		arrFrom.DeepCloneTo(arrTo);
		Assert.Equal(4, arrTo.Length);
		Assert.NotEqual<object>(c1, arrTo[0]);
		Assert.Equal(arrTo[1], arrTo[0]);
		Assert.Equal(arrTo[2], arrTo[1]);
		Assert.NotEqual(c1, arrTo[2].A);
		Assert.Equal(arrTo[2].B, arrTo[2].A);
		Assert.Null(arrTo[3]);
	}
}
