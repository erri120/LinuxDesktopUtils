using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Tmds.DBus.Protocol;

namespace LinuxDesktopUtils.XDGDesktopPortal;

public partial class ScreenshotPortal
{
    /// <summary>
    /// Results of <see cref="ScreenshotPortal.ScreenshotAsync"/>.
    /// </summary>
    [PublicAPI]
    public record ScreenshotResults
    {
        /// <summary>
        /// The URI of the screenshot.
        /// </summary>
        public required Uri Uri { get; init; }

        internal static ScreenshotResults From(Dictionary<string, VariantValue> varDict)
        {
            var image = varDict["uri"].GetString();
            var uri = new Uri(image, UriKind.Absolute);

            return new ScreenshotResults
            {
                Uri = uri,
            };
        }
    }
}
