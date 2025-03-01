using JetBrains.Annotations;

namespace LinuxDesktopUtils.XDGDesktopPortal;

public partial class NetworkMonitorPortal
{
    /// <summary>
    /// Represents the connectivity status returned by the network monitor.
    /// </summary>
    [PublicAPI]
    public enum ConnectivityStatus : uint
    {
        /// <summary>
        /// The host is not configured with a route to the internet.
        /// </summary>
        LocalOnly = 1,

        /// <summary>
        /// The host is connected to a network, but can’t reach the full internet.
        /// </summary>
        Limited = 2,

        /// <summary>
        /// The host is behind a captive portal and cannot reach the full internet.
        /// </summary>
        Captive = 3,

        /// <summary>
        /// The host connected to a network, and can reach the full internet.
        /// </summary>
        Full = 4,
    }
}

