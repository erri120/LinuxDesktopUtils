using System;
using System.Net;
using System.Threading.Tasks;
using JetBrains.Annotations;
using R3;
using Tmds.DBus.SourceGenerator;

namespace LinuxDesktopUtils.XDGDesktopPortal;

/// <summary>
/// Network monitoring portal.
///
/// The NetworkMonitor portal provides network status information to sandboxed applications.
/// </summary>
/// <remarks>
/// https://flatpak.github.io/xdg-desktop-portal/docs/doc-org.freedesktop.portal.NetworkMonitor.html.
/// </remarks>
[PublicAPI]
public partial class NetworkMonitorPortal : IPortal
{
    private readonly DesktopPortalConnectionManager _connectionManager;
    private readonly OrgFreedesktopPortalNetworkMonitorProxy _instance;
    private readonly uint _version;

    /// <inheritdoc/>
    uint IPortal.Version => _version;

    private NetworkMonitorPortal(
        DesktopPortalConnectionManager connectionManager,
        OrgFreedesktopPortalNetworkMonitorProxy instance,
        uint version)
    {
        _connectionManager = connectionManager;
        _instance = instance;
        _version = version;
    }

    internal static async ValueTask<NetworkMonitorPortal> CreateAsync(DesktopPortalConnectionManager connectionManager)
    {
        var instance = new OrgFreedesktopPortalNetworkMonitorProxy(connectionManager.GetConnection(), destination: DBusHelper.BusName, path: DBusHelper.ObjectPath);
        var version = await instance.GetVersionPropertyAsync().ConfigureAwait(false);

        return new NetworkMonitorPortal(connectionManager, instance, version);
    }

    /// <summary>
    /// Returns whether the network is considered available. That is, whether the system as a default route for at least one of IPv4 or IPv6.
    /// </summary>
    /// <exception cref="PortalVersionException">Thrown if the installed portal backend doesn't support this method.</exception>
    public async Task<bool> GetAvailableAsync()
    {
        const uint addedInVersion = 2;
        PortalVersionException.ThrowIf(requiredVersion: addedInVersion, availableVersion: _version);

        var result = await _instance.GetAvailableAsync().ConfigureAwait(false);
        return result;
    }

    /// <summary>
    /// Returns whether the network is considered metered. That is, whether the system as traffic flowing through the default connection that is subject ot limitations by service providers.
    /// </summary>
    /// <exception cref="PortalVersionException">Thrown if the installed portal backend doesn't support this method.</exception>
    public async Task<bool> GetMeteredAsync()
    {
        const uint addedInVersion = 2;
        PortalVersionException.ThrowIf(requiredVersion: addedInVersion, availableVersion: _version);

        var result = await _instance.GetMeteredAsync().ConfigureAwait(false);
        return result;
    }

    /// <summary>
    /// Returns more detailed information about the host’s network connectivity.
    /// </summary>
    /// <exception cref="PortalVersionException">Thrown if the installed portal backend doesn't support this method.</exception>
    /// <exception cref="NotSupportedException">Thrown if the portal returned an unknown connectivity status.</exception>
    public async Task<ConnectivityStatus> GetConnectivityAsync()
    {
        const uint addedInVersion = 2;
        PortalVersionException.ThrowIf(requiredVersion: addedInVersion, availableVersion: _version);

        var result = await _instance.GetConnectivityAsync().ConfigureAwait(false);
        if (result is < (uint)ConnectivityStatus.LocalOnly or > (uint)ConnectivityStatus.Full)
            throw new NotSupportedException($"Portal returned invalid connectivity status: `{result}`");

        return (ConnectivityStatus)result;
    }

    /// <summary>
    /// Returns values from <see cref="GetAvailableAsync"/>, <see cref="GetMeteredAsync"/>, and <see cref="GetConnectivityAsync"/>
    /// in one call.
    /// </summary>
    /// <exception cref="PortalVersionException">Thrown if the installed portal backend doesn't support this method.</exception>
    /// <exception cref="NotSupportedException">Thrown if the portal returned an unknown connectivity status.</exception>
    public async Task<GetStatusResults> GetStatusAsync()
    {
        const uint addedInVersion = 3;
        PortalVersionException.ThrowIf(requiredVersion: addedInVersion, availableVersion: _version);

        var values = await _instance.GetStatusAsync().ConfigureAwait(false);
        var res = GetStatusResults.From(values);
        return res;
    }

    /// <inheritdoc cref="CanReachAsync(System.String, System.UInt32)"/>
    public Task<bool> CanReachAsync(Uri uri) => CanReachAsync(uri.Host, (uint)uri.Port);

    /// <inheritdoc cref="CanReachAsync(System.String, System.UInt32)"/>
    public Task<bool> CanReachAsync(IPAddress address, uint port) => CanReachAsync(address.ToString(), port);

    /// <summary>
    /// Returns whether the given hostname is believed to be reachable.
    /// </summary>
    /// <exception cref="PortalVersionException">Thrown if the installed portal backend doesn't support this method.</exception>
    public async Task<bool> CanReachAsync(string hostname, uint port)
    {
        const uint addedInVersion = 3;
        PortalVersionException.ThrowIf(requiredVersion: addedInVersion, availableVersion: _version);

        var res = await _instance.CanReachAsync(hostname, port).ConfigureAwait(false);
        return res;
    }

    /// <summary>
    /// Observes changes to the network configuration.
    /// </summary>
    public async ValueTask<Observable<Unit>> ObserveChangedAsync()
    {
        var subject = new Subject<Unit>();

        var disposable = await _instance.WatchChangedAsync(exception =>
        {
            if (exception is null) subject.OnNext(Unit.Default);
            else subject.OnErrorResume(exception);
        }, emitOnCapturedContext: false).ConfigureAwait(false);

        return Observable.Create<Unit, ValueTuple<Subject<Unit>, IDisposable>>((subject, disposable), static (observer, state) =>
        {
            var (subject, disposable1) = state;
            var disposable2 = subject.Subscribe(observer);
            return Disposable.Combine(disposable1, disposable2);
        });
    }
}
