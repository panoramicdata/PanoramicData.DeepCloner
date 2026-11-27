using System;
using System.Runtime.CompilerServices;

namespace PanoramicData.DeepCloner.Helpers;

internal sealed partial class DeepCloneState
{
    private sealed class MiniDictionary
	{
		private struct Entry : IEquatable<Entry>
		{
			public int HashCode;
			public int Next;
			public object Key;
			public object Value;

			public bool Equals(Entry other)
			{
				return HashCode == other.HashCode
					&& Next == other.Next
					&& ReferenceEquals(Key, other.Key)
					&& ReferenceEquals(Value, other.Value);
			}

			public override bool Equals(object? obj)
			{
				return obj is Entry other && Equals(other);
			}

			public override int GetHashCode()
			{
				return HashCode;
			}
		}

     private int[]? _buckets;
		private Entry[]? _entries;
		private int _count;


		public MiniDictionary() : this(5)
		{
		}

		public MiniDictionary(int capacity)
		{
			if (capacity > 0)
			{
				Initialize(capacity);
			}
		}

     public object? FindEntry(object key)
		{
			if (_buckets != null)
			{
				var hashCode = RuntimeHelpers.GetHashCode(key) & 0x7FFFFFFF;
                var entries1 = _entries!;
				for (var i = _buckets[hashCode % _buckets.Length]; i >= 0; i = entries1[i].Next)
				{
					if (entries1[i].HashCode == hashCode && ReferenceEquals(entries1[i].Key, key))
					{
						return entries1[i].Value;
					}
				}
			}

			return null;
		}

		private void Initialize(int size)
		{
			_buckets = new int[size];
			Array.Fill(_buckets, -1);
			_entries = new Entry[size];
		}

		public void Insert(object key, object value)
		{
			EnsureInitialized();
			var hashCode = RuntimeHelpers.GetHashCode(key) & 0x7FFFFFFF;
			var targetBucket = EnsureCapacity(hashCode);
			var entries1 = _entries!;

			var index = _count;
			_count++;

			entries1[index].HashCode = hashCode;
			entries1[index].Next = _buckets![targetBucket];
			entries1[index].Key = key;
			entries1[index].Value = value;
			_buckets[targetBucket] = index;
		}

		private void EnsureInitialized()
		{
			if (_buckets == null)
			{
				Initialize(0);
			}
		}

		private int EnsureCapacity(int hashCode)
		{
			var targetBucket = hashCode % _buckets!.Length;
			if (_count == _entries!.Length)
			{
				Resize();
				targetBucket = hashCode % _buckets!.Length;
			}
			return targetBucket;
		}

		private void Resize()
		{
			Resize(MiniDictionaryPrimes.ExpandPrime(_count));
		}

		private void Resize(int newSize)
		{
			var newBuckets = new int[newSize];
			Array.Fill(newBuckets, -1);
			var newEntries = new Entry[newSize];
         Array.Copy(_entries!, 0, newEntries, 0, _count);

			for (var i = 0; i < _count; i++)
			{
				if (newEntries[i].HashCode >= 0)
				{
					var bucket = newEntries[i].HashCode % newSize;
					newEntries[i].Next = newBuckets[bucket];
					newBuckets[bucket] = i;
				}
			}

			_buckets = newBuckets;
			_entries = newEntries;
		}
	}
}
