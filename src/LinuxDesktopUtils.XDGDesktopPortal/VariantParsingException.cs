using System;
using JetBrains.Annotations;
using Tmds.DBus.Protocol;

namespace LinuxDesktopUtils.XDGDesktopPortal;

/// <summary>
/// Exception thrown when parsing <see cref="VariantValue"/> fails.
/// </summary>
[PublicAPI]
public class VariantParsingException : Exception
{
    internal VariantParsingException(string msg) : base(msg) { }

    internal static void ExpectType(VariantValue variantValue, VariantValueType expectedType)
    {
        if (variantValue.Type == expectedType) return;
        throw new VariantParsingException($"Expected type `{expectedType}` but found `{variantValue.Type}`");
    }

    internal static void ExpectItemType(VariantValue variantValue, VariantValueType expectedItemType)
    {
        if (variantValue.ItemType == expectedItemType) return;
        throw new VariantParsingException($"Expected item type `{expectedItemType}` but found `{variantValue.ItemType}`");
    }

    internal static void ExpectCount(VariantValue variantValue, int expectedCount)
    {
        if (variantValue.Count == expectedCount) return;
        throw new VariantParsingException($"Expected count `{expectedCount}` but found `{variantValue.Count}`");
    }

    internal static void ExpectArray(VariantValue variantValue, VariantValueType expectedItemType, Optional<int> expectedCount = default)
    {
        ExpectType(variantValue, VariantValueType.Array);
        ExpectItemType(variantValue, expectedItemType);
        if (expectedCount.HasValue) ExpectCount(variantValue, expectedCount.Value);
    }

    internal static void ExpectStruct(VariantValue variantValue, int expectedCount)
    {
        ExpectType(variantValue, VariantValueType.Struct);
        ExpectCount(variantValue, expectedCount);
    }
}
