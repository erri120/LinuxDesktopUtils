using System;
using JetBrains.Annotations;

namespace LinuxDesktopUtils.XDGDesktopPortal;

/// <summary>
/// Exception thrown when the portal is unavailable.
/// </summary>
[PublicAPI]
public class PortalUnavailableException : PortalException
{
    internal PortalUnavailableException(Type portalType, Exception innerException)
        : base($"Portal `{portalType}` is unavailable", innerException) { }
}
