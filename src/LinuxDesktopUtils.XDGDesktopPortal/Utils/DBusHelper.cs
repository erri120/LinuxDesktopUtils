using System;
using System.Collections.Generic;
using System.Globalization;
using Tmds.DBus.Protocol;

namespace LinuxDesktopUtils.XDGDesktopPortal;

internal static class DBusHelper
{
    public const string BusName = "org.freedesktop.portal.Desktop";
    public const string ObjectPath = "/org/freedesktop/portal/desktop";

    public static readonly Dictionary<string, Variant> EmptyVarDict = new(StringComparer.OrdinalIgnoreCase);

    internal static string CreateHandleToken() => $"LinuxDesktopUtils_{Random.Shared.Next().ToString(CultureInfo.InvariantCulture)}";

    internal static string UniqueNameToSenderName(ReadOnlySpan<char> uniqueName)
    {
        // callers unique name, with the initial ':' removed and all '.' replaced by '_'
        if (uniqueName.Length > 2 && uniqueName[0] == ':')
            uniqueName = uniqueName[1..];

        Span<char> dest = stackalloc char[uniqueName.Length];
        uniqueName.Replace(dest, '.', '_');

        return dest.ToString();
    }
}
