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
        if (KnownTypes.TryGetValue(type, out bool isSafe))
        {
            return isSafe;
        }

        if (IsSafeSpecialType(type))
        {
            KnownTypes.TryAdd(type, true);
            return true;
        }

        // classes are always unsafe (we should copy it fully to count references)
        if (!type.IsValueType())
        {
            KnownTypes.TryAdd(type, false);
            return false;
        }

        if (processingTypes == null)
        {
            processingTypes = new HashSet<Type>();
        }

        // structs cannot have a loops, but check it anyway
        processingTypes.Add(type);

        if (!HasOnlySafeFields(type, processingTypes))
        {
            KnownTypes.TryAdd(type, false);
            return false;
        }

        KnownTypes.TryAdd(type, true);
        return true;
    }

    private static bool IsSafeSpecialType(Type type)
    {
        // enums are safe
        // pointers (e.g. int*) are unsafe, but we cannot do anything with it except blind copy
        if (type.IsEnum() || type.IsPointer)
        {
            return true;
        }

        var fullName = type.FullName;
        if (fullName == null)
        {
            return false;
        }

        if (fullName == "Microsoft.EntityFrameworkCore.Internal.ConcurrencyDetector")
        {
            return true;
        }

        return IsSafeFullName(type, fullName);
    }

    private static bool IsSafeFullName(Type type, string fullName)
    {
        // do not copy db null
        if (fullName.StartsWith("System.DBNull", StringComparison.Ordinal))
        {
            return true;
        }

        if (fullName.StartsWith("System.RuntimeType", StringComparison.Ordinal))
        {
            return true;
        }

        if (fullName.StartsWith("System.Reflection.", StringComparison.Ordinal)
                && Equals(type.GetTypeInfo().Assembly, typeof(PropertyInfo).GetTypeInfo().Assembly))
        {
            return true;
        }

        // better not to touch ms dependency injection
        if (fullName.StartsWith("Microsoft.Extensions.DependencyInjection.", StringComparison.Ordinal))
        {
            return true;
        }

        // default comparers should not be cloned due possible comparison EqualityComparer<T>.Default == comparer
        if (fullName.Contains("EqualityComparer"))
        {
            return IsSafeEqualityComparer(fullName);
        }

        return false;
    }

    private static bool IsSafeEqualityComparer(string fullName)
    {
        return fullName.StartsWith("System.Collections.Generic.GenericEqualityComparer`", StringComparison.Ordinal)
                || fullName.StartsWith("System.Collections.Generic.ObjectEqualityComparer`", StringComparison.Ordinal)
                || fullName.StartsWith("System.Collections.Generic.EnumEqualityComparer`", StringComparison.Ordinal)
                || fullName.StartsWith("System.Collections.Generic.NullableEqualityComparer`", StringComparison.Ordinal)
                || fullName == "System.Collections.Generic.ByteEqualityComparer";
    }

    private static bool HasOnlySafeFields(Type type, HashSet<Type> processingTypes)
    {
        var fieldInfos = GetAllFields(type);

        foreach (var fieldInfo in fieldInfos)
        {
            // type loop
            var fieldType = fieldInfo.FieldType;
            if (processingTypes.Contains(fieldType))
            {
                continue;
            }

            // not safe and not not safe. we need to go deeper
            if (!CanReturnSameType(fieldType, processingTypes))
            {
                return false;
            }
        }

        return true;
    }

    private static List<FieldInfo> GetAllFields(Type type)
    {
        List<FieldInfo> fi = new List<FieldInfo>();
        var tp = type;
        do
        {
            fi.AddRange(tp.GetAllFields());
            tp = tp.BaseType();
        }
        while (tp != null);

        return fi;
    }

    public static bool CanReturnSameObject(Type type)
    {
        return CanReturnSameType(type, null);
    }
}
