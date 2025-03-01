using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Tmds.DBus.Protocol;

namespace LinuxDesktopUtils.XDGDesktopPortal;

public partial class OpenUriPortal
{
    /// <summary>
    /// Options for <see cref="OpenUriPortal.OpenFileInDirectoryAsync"/>.
    /// </summary>
    [PublicAPI]
    public record OpenFileInDirectoryOptions : IPortalOptions
    {
        internal readonly string HandleToken = DBusHelper.CreateHandleToken();

        /// <inheritdoc/>
        public Dictionary<string, VariantValue> ToVarDict()
        {
            return new Dictionary<string, VariantValue>(StringComparer.OrdinalIgnoreCase)
            {
                { "handle_token", HandleToken },
            };
        }
    }
}
