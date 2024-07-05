using System;
using System.Collections.Generic;
using Tmds.DBus.Protocol;

namespace LinuxDesktopUtils.XDGDesktopPortal;

internal static class DBusHelper
{
    public const string BusName = "org.freedesktop.portal.Desktop";
    public const string ObjectPath = "/org/freedesktop/portal/desktop";

    public static readonly Dictionary<string, Variant> EmptyVarDict = new(StringComparer.OrdinalIgnoreCase);
}
