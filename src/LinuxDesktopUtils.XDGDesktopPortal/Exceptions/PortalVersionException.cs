using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace LinuxDesktopUtils.XDGDesktopPortal;

/// <summary>
/// Exception for version mismatches.
/// </summary>
[PublicAPI]
public class PortalVersionException : PortalException
{
    internal PortalVersionException(string name, uint requiredVersion, uint availableVersion)
        : base($"Unable to use `{name}` because it requires version {requiredVersion} but the installed portal only supports version {availableVersion}") { }

    internal static void ThrowIf(uint requiredVersion, uint availableVersion, [CallerMemberName] string? methodName = null)
    {
        if (availableVersion >= requiredVersion) return;
        throw new PortalVersionException(methodName ?? string.Empty, requiredVersion, availableVersion);
    }
}
