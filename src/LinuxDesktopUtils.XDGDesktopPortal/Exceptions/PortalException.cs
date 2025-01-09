using System;
using JetBrains.Annotations;

namespace LinuxDesktopUtils.XDGDesktopPortal;

/// <summary>
/// Represents an exception for a portal.
/// </summary>
[PublicAPI]
public class PortalException : Exception
{
    internal PortalException() { }

    internal PortalException(string message) : base(message) { }

    internal PortalException(string message, Exception innerException) : base(message, innerException) { }
}
