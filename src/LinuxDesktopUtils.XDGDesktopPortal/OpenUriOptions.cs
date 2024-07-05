using System.Collections.Generic;
using JetBrains.Annotations;
using Tmds.DBus.Protocol;

namespace LinuxDesktopUtils.XDGDesktopPortal;

/// <summary>
/// Options for <see cref="OpenUriPortal.OpenUriAsync"/>.
/// </summary>
[PublicAPI]
public record OpenUriOptions : IPortalOptions
{
    /// <summary>
    /// Default values.
    /// </summary>
    public static readonly OpenUriOptions Default = new();

    /// <summary>
    /// Whether to allow the chosen application to write to the file.
    /// </summary>
    /// <remarks>
    /// This key only takes effect the uri points to a local file that is exported in the document portal, and the chosen application is sandboxed itself.
    /// </remarks>
    public bool Writeable { get; init; }

    /// <summary>
    /// Whether to ask the user to choose an app. If this is false, the portal may use a default or pick the last choice.
    /// </summary>
    /// <remarks>
    /// The ask option was introduced in version 3 of the interface.
    /// </remarks>
    public bool Ask { get; init; }

    /// <inheritdoc/>
    public Dictionary<string, Variant> ToVarDict()
    {
        return new Dictionary<string, Variant>(System.StringComparer.OrdinalIgnoreCase)
        {
            { "writable", new Variant(Writeable) },
            { "ask", new Variant(Ask) },
        };
    }
}
