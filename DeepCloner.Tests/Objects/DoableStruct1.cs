#pragma warning disable S1104 // Public fields are deliberate: these fixtures exist to test field cloning
namespace PanoramicData.DeepCloner.Test.Objects;

public struct DoableStruct1 : IDoable, IEquatable<DoableStruct1>
{
	public int X;

	public int Do()
	{
		return ++X;
	}

	public readonly bool Equals(DoableStruct1 other) => EqualityComparer<int>.Default.Equals(X, other.X);

	public override readonly bool Equals(object? obj) => obj is DoableStruct1 other && Equals(other);

	public override readonly int GetHashCode() => EqualityComparer<int>.Default.GetHashCode(X);
}
