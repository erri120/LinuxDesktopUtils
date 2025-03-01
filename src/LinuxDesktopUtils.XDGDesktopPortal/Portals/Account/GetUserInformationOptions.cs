using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Tmds.DBus.Protocol;

namespace LinuxDesktopUtils.XDGDesktopPortal;

public partial class AccountPortal
{
    /// <summary>
    /// Options for <see cref="AccountPortal.GetUserInformationAsync"/>.
    /// </summary>
    [PublicAPI]
    public record GetUserInformationOptions : IPortalOptions
    {
        internal readonly string HandleToken = DBusHelper.CreateHandleToken();

        /// <summary>
        /// A string that can be shown in the dialog to explain why the information is needed.
        /// This should be a complete sentence that explains what the application will do with the returned information.
        /// </summary>
        /// <example>
        /// Allows your personal information to be included with recipes you share with your friends
        /// </example>
        public string? Reason { get; init; }

        /// <inheritdoc/>
        public Dictionary<string, VariantValue> ToVarDict()
        {
            var varDict = new Dictionary<string, VariantValue>(StringComparer.OrdinalIgnoreCase)
            {
                { "handle_token", HandleToken },
            };

            if (!string.IsNullOrEmpty(Reason)) varDict.Add("reason", Reason);
            return varDict;
        }
    }
}
