using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Tmds.DBus.Protocol;

namespace LinuxDesktopUtils.XDGDesktopPortal;

/// <summary>
/// Options for <see cref="OpenUriPortal.OpenFileInDirectoryAsync"/>.
/// </summary>
[PublicAPI]
public class OpenFileInDirectoryOptions : IPortalOptions
{
    internal readonly string HandleToken = DBusHelper.CreateHandleToken();

    /// <inheritdoc/>
    public Dictionary<string, Variant> ToVarDict()
    {
        return new Dictionary<string, Variant>(StringComparer.OrdinalIgnoreCase)
        {
            { "handle_token", new Variant(HandleToken) },
        };
    }
}
