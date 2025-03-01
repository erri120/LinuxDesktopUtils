using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Tmds.DBus.Protocol;

namespace LinuxDesktopUtils.XDGDesktopPortal;

public partial class NetworkMonitorPortal
{
    /// <summary>
    /// Results of <see cref="NetworkMonitorPortal.GetStatusAsync"/>.
    /// </summary>
    [PublicAPI]
    public record GetStatusResults
    {
        /// <summary>
        /// Whether the network is available.
        /// </summary>
        public bool IsAvailable { get; set; }

        /// <summary>
        /// Whether the network is metered.
        /// </summary>
        public bool IsMetered { get; set; }

        /// <summary>
        /// The level of connectivity.
        /// </summary>
        public ConnectivityStatus Status { get; set; }

        internal static GetStatusResults From(Dictionary<string, VariantValue> varDict)
        {
            var res = new GetStatusResults();

            if (varDict.TryGetValue("available", out var availableValue))
            {
                var isAvailable = availableValue.GetBool();
                res.IsAvailable = isAvailable;
            }

            if (varDict.TryGetValue("metered", out var meteredValue))
            {
                var isMetered = meteredValue.GetBool();
                res.IsMetered = isMetered;
            }

            if (varDict.TryGetValue("connectivity", out var connectivityValue))
            {
                var rawStatus = connectivityValue.GetUInt32();
                if (rawStatus is < (uint)ConnectivityStatus.LocalOnly or > (uint)ConnectivityStatus.Full)
                    throw new NotSupportedException($"Portal returned invalid connectivity status: `{rawStatus}`");

                res.Status = (ConnectivityStatus)rawStatus;
            }

            return res;
        }
    }
}

