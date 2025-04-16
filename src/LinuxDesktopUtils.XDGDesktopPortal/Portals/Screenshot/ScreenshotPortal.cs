using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Tmds.DBus.SourceGenerator;

namespace LinuxDesktopUtils.XDGDesktopPortal;

/// <summary>
/// Portal for taking screenshots.
///
/// This simple portal lets sandboxed applications request a screenshot.
/// </summary>
/// <remarks>
/// https://flatpak.github.io/xdg-desktop-portal/docs/doc-org.freedesktop.portal.Screenshot.html
/// </remarks>
[PublicAPI]
public partial class ScreenshotPortal : IPortal
{
    private readonly DesktopPortalConnectionManager _connectionManager;
    private readonly OrgFreedesktopPortalScreenshotProxy _instance;
    private readonly uint _version;

    /// <inheritdoc/>
    uint IPortal.Version => _version;

    private ScreenshotPortal(
        DesktopPortalConnectionManager connectionManager,
        OrgFreedesktopPortalScreenshotProxy instance,
        uint version)
    {
        _connectionManager = connectionManager;
        _instance = instance;
        _version = version;
    }

    internal static async ValueTask<ScreenshotPortal> CreateAsync(DesktopPortalConnectionManager connectionManager)
    {
        var instance = new OrgFreedesktopPortalScreenshotProxy(connectionManager.GetConnection(), destination: DBusHelper.BusName, path: DBusHelper.ObjectPath);
        var version = await instance.GetVersionPropertyAsync().ConfigureAwait(false);

        return new ScreenshotPortal(connectionManager, instance, version);
    }

    /// <summary>
    /// Takes a screenshot of the entire desktop.
    /// </summary>
    /// <param name="windowIdentifier">Identifier of the parent window.</param>
    /// <param name="options">Additional options.</param>
    /// <param name="cancellationToken">CancellationToken to cancel the request.</param>
    /// <exception cref="PortalVersionException">Thrown if the installed portal backend doesn't support this method.</exception>
    public async Task<Response<ScreenshotResults>> ScreenshotAsync(
        Optional<WindowIdentifier> windowIdentifier = default,
        ScreenshotOptions? options = null,
        Optional<CancellationToken> cancellationToken = default)
    {
        const uint addedInVersion = 1;
        PortalVersionException.ThrowIf(requiredVersion: addedInVersion, availableVersion: _version);

        options ??= new ScreenshotOptions();

        var request = await _connectionManager.CreateRequestAsync(
            options.HandleToken,
            resultsDelegate: ScreenshotResults.From,
            cancellationToken
        ).ConfigureAwait(false);

        await using var _ = request.ConfigureAwait(false);

        var returnedRequestObjectPath = await _instance.ScreenshotAsync(
            parentWindow: _connectionManager.GetWindowIdentifier(windowIdentifier),
            options: options.ToVarDict()
        ).ConfigureAwait(false);

        await request.UpdateAsync(returnedRequestObjectPath).ConfigureAwait(false);
        return await request.GetTask().ConfigureAwait(false);
    }
}

