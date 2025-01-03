using DeepCloner.Helpers;
using System.Reflection;

namespace DeepCloner.Test;

public class BaseTest
{
	public BaseTest(bool isSafeInit)
	{
		SwitchTo(isSafeInit);
	}

	public static void SwitchTo(bool isSafeInit)
	{
		typeof(ShallowObjectCloner).GetMethod("SwitchTo", BindingFlags.NonPublic | BindingFlags.Static)
								.Invoke(null, [isSafeInit]);
	}
}
