using System;
using System.Runtime.CompilerServices;

namespace PanoramicData.DeepCloner.Helpers;

internal sealed partial class DeepCloneState
{
  private MiniDictionary? _loops;

	private readonly object[] _baseFromTo = new object[6];

	private int _idx;

  public object? GetKnownRef(object from)
	{
		// this is faster than call Dictionary from begin
		// also, small poco objects does not have a lot of references
		var baseFromTo = _baseFromTo;
		if (ReferenceEquals(from, baseFromTo[0])) { return baseFromTo[3]; }
		if (ReferenceEquals(from, baseFromTo[1])) { return baseFromTo[4]; }
		if (ReferenceEquals(from, baseFromTo[2])) { return baseFromTo[5]; }
		if (_loops == null)
		{
			return null;
		}

		return _loops.FindEntry(from);
	}

	public void AddKnownRef(object from, object to)
	{
		if (_idx < 3)
		{
			_baseFromTo[_idx] = from;
			_baseFromTo[_idx + 3] = to;
			_idx++;
			return;
		}

		if (_loops == null)
		{
			_loops = new MiniDictionary();
		}
		_loops.Insert(from, to);
	}
}
