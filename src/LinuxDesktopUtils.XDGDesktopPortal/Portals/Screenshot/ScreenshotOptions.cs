using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Tmds.DBus.Protocol;

namespace LinuxDesktopUtils.XDGDesktopPortal;

public partial class ScreenshotPortal
{
    /// <summary>
    /// Options for <see cref="ScreenshotPortal.ScreenshotAsync"/>.
    /// </summary>
    [PublicAPI]
    public record ScreenshotOptions : IPortalOptions
    {
        internal readonly string HandleToken = DBusHelper.CreateHandleToken();

        /// <summary>
        /// Whether the dialog should be modal.
        /// </summary>
        public bool IsDialogModal { get; init; } = true;

        /// <summary>
        /// Hint whether the dialog should offer customization before taking a screenshot.
        /// </summary>
        /// <remarks>
        /// Added in version 2.
        /// </remarks>
        public bool IsDialogInteractive { get; init; }

        /// <inheritdoc/>
        public Dictionary<string, VariantValue> ToVarDict()
        {
            var varDict = new Dictionary<string, VariantValue>(StringComparer.OrdinalIgnoreCase)
            {
                { "handle_token", HandleToken },
                { "modal", IsDialogModal },
                { "interactive", IsDialogInteractive },
            };

            return varDict;
        }
    }
}
