using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace PanoramicData.DeepCloner.Helpers;

/// <summary>
/// Safe types are types, which can be copied without real cloning. e.g. simple structs or strings (it is immutable)
/// </summary>
internal static class DeepClonerSafeTypes
{
    internal static readonly ConcurrentDictionary<Type, bool> KnownTypes = new ConcurrentDictionary<Type, bool>();

    static DeepClonerSafeTypes()
    {
        foreach (
             var x in
                 new[]
                     {
                        typeof(byte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong),
                        typeof(float), typeof(double), typeof(decimal), typeof(char), typeof(string), typeof(bool), typeof(DateTime),
                        typeof(IntPtr), typeof(UIntPtr), typeof(Guid),
						// do not clone such native type
						Type.GetType("System.RuntimeType"),
                        Type.GetType("System.RuntimeTypeHandle"),
                        StringComparer.Ordinal.GetType(),
                        StringComparer.CurrentCulture.GetType(), // CultureAwareComparer - can be same
#if !NETCORE
						typeof(DBNull)
#endif
                     })
        {
            if (x != null)
            {
                KnownTypes.TryAdd(x, true);
            }
        }
    }

    private static bool CanReturnSameType(Type type, HashSet<Type>? processingTypes)
    {
        var fullName = type.FullName;
        bool isSafe;
        if (KnownTypes.TryGetValue(type, out isSafe))
            return isSafe;

        // enums are safe
        // pointers (e.g. int*) are unsafe, but we cannot do anything with it except blind copy
        if (type.IsEnum() || type.IsPointer)
        {
            KnownTypes.TryAdd(type, true);
            return true;
        }


        // do not copy db null
        if (fullName != null && fullName.StartsWith("System.DBNull", StringComparison.Ordinal))
        {
            KnownTypes.TryAdd(type, true);
            return true;
        }

        if (fullName != null && fullName.StartsWith("System.RuntimeType", StringComparison.Ordinal))
        {
            KnownTypes.TryAdd(type, true);
            return true;
        }

        if (fullName != null && fullName.StartsWith("System.Reflection.", StringComparison.Ordinal) && Equals(type.GetTypeInfo().Assembly, typeof(PropertyInfo).GetTypeInfo().Assembly))
        {
            KnownTypes.TryAdd(type, true);
            return true;
        }

        // better not to touch ms dependency injection
        if (fullName != null && fullName.StartsWith("Microsoft.Extensions.DependencyInjection.", StringComparison.Ordinal))
        {
            KnownTypes.TryAdd(type, true);
            return true;
        }

        if (fullName == "Microsoft.EntityFrameworkCore.Internal.ConcurrencyDetector")
        {
            KnownTypes.TryAdd(type, true);
            return true;
        }

        // default comparers should not be cloned due possible comparison EqualityComparer<T>.Default == comparer
        if (fullName != null && fullName.Contains("EqualityComparer"))
        {
            if (fullName.StartsWith("System.Collections.Generic.GenericEqualityComparer`", StringComparison.Ordinal)
                   || fullName.StartsWith("System.Collections.Generic.ObjectEqualityComparer`", StringComparison.Ordinal)
                   || fullName.StartsWith("System.Collections.Generic.EnumEqualityComparer`", StringComparison.Ordinal)
                   || fullName.StartsWith("System.Collections.Generic.NullableEqualityComparer`", StringComparison.Ordinal)
                   || fullName == "System.Collections.Generic.ByteEqualityComparer")
            {
                KnownTypes.TryAdd(type, true);
                return true;
            }
        }

        // classes are always unsafe (we should copy it fully to count references)
        if (!type.IsValueType())
        {
            KnownTypes.TryAdd(type, false);
            return false;
        }

        if (processingTypes == null)
            processingTypes = new HashSet<Type>();

        // structs cannot have a loops, but check it anyway
        processingTypes.Add(type);

        List<FieldInfo> fi = new List<FieldInfo>();
        var tp = type;
        do
        {
            fi.AddRange(tp.GetAllFields());
            tp = tp.BaseType();
        }
        while (tp != null);

        foreach (var fieldInfo in fi)
        {
            // type loop
            var fieldType = fieldInfo.FieldType;
            if (processingTypes.Contains(fieldType))
                continue;

            // not safe and not not safe. we need to go deeper
            if (!CanReturnSameType(fieldType, processingTypes))
            {
                KnownTypes.TryAdd(type, false);
                return false;
            }
        }

        KnownTypes.TryAdd(type, true);
        return true;
    }

    // not used anymore
    /*/// <summary>
	/// Classes with only safe fields are safe for ShallowClone (if they root objects for copying)
	/// </summary>
	private static bool CanCopyClassInShallow(Type type)
	{
		// do not do this anything for struct and arrays
		if (!type.IsClass() || type.IsArray)
		{
			return false;
		}

		List<FieldInfo> fi = new List<FieldInfo>();
		var tp = type;
		do
		{
			fi.AddRange(tp.GetAllFields());
			tp = tp.BaseType();
		}
		while (tp != null);

		if (fi.Any(fieldInfo => !CanReturnSameType(fieldInfo.FieldType, null)))
		{
			return false;
		}

		return true;
	}*/

    public static bool CanReturnSameObject(Type type)
    {
        return CanReturnSameType(type, null);
    }
}
