using JetBrains.Annotations;

namespace LinuxDesktopUtils.XDGDesktopPortal;

/// <summary>
/// Represents a portal instance.
/// </summary>
[PublicAPI]
internal interface IPortal
{
    /// <summary>
    /// Gets the implemented version of the portal.
    /// </summary>
    uint Version { get; }
}
