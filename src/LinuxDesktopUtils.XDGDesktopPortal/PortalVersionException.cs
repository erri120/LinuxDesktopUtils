using System;
using JetBrains.Annotations;

namespace LinuxDesktopUtils.XDGDesktopPortal;

/// <summary>
/// Exception for version mismatches.
/// </summary>
[PublicAPI]
public class PortalVersionException : Exception
{
    internal PortalVersionException(string name, uint requiredVersion, uint availableVersion)
        : base($"Unable to use `{name}` because it requires version {requiredVersion} but the installed portal only supports version {availableVersion}") { }
}
