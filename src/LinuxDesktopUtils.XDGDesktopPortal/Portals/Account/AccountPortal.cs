using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Tmds.DBus.SourceGenerator;

namespace LinuxDesktopUtils.XDGDesktopPortal;

/// <summary>
/// Portal for obtaining information about the user.
///
/// This simple interface lets sandboxed applications query basic information about the user, like their name and avatar photo.
/// </summary>
/// <remarks>
/// https://flatpak.github.io/xdg-desktop-portal/docs/doc-org.freedesktop.portal.Account.html#
/// </remarks>
[PublicAPI]
public partial class AccountPortal : IPortal
{
    private readonly DesktopPortalConnectionManager _connectionManager;
    private readonly OrgFreedesktopPortalAccountProxy _instance;
    private readonly uint _version;

    /// <inheritdoc/>
    uint IPortal.Version => _version;

    private AccountPortal(
        DesktopPortalConnectionManager connectionManager,
        OrgFreedesktopPortalAccountProxy instance,
        uint version)
    {
        _connectionManager = connectionManager;
        _instance = instance;
        _version = version;
    }

    internal static async ValueTask<AccountPortal> CreateAsync(DesktopPortalConnectionManager connectionManager)
    {
        var instance = new OrgFreedesktopPortalAccountProxy(connectionManager.GetConnection(), destination: DBusHelper.BusName, path: DBusHelper.ObjectPath);
        var version = await instance.GetVersionPropertyAsync().ConfigureAwait(false);

        return new AccountPortal(connectionManager, instance, version);
    }

    /// <summary>
    /// Gets information about the user.
    /// </summary>
    /// <param name="windowIdentifier">Identifier of the parent window.</param>
    /// <param name="options">Additional options.</param>
    /// <param name="cancellationToken">CancellationToken to cancel the request.</param>
    /// <exception cref="PortalVersionException">Thrown if the installed portal backend doesn't support this method.</exception>
    public async Task<Response<GetUserInformationResults>> GetUserInformationAsync(
        Optional<WindowIdentifier> windowIdentifier = default,
        GetUserInformationOptions? options = null,
        Optional<CancellationToken> cancellationToken = default)
    {
        const uint addedInVersion = 1;
        PortalVersionException.ThrowIf(requiredVersion: addedInVersion, availableVersion: _version);
        if (cancellationToken.HasValue) cancellationToken.Value.ThrowIfCancellationRequested();

        options ??= new GetUserInformationOptions();

        var request = await _connectionManager.CreateRequestAsync(
            options.HandleToken,
            resultsDelegate: GetUserInformationResults.From,
            cancellationToken
        ).ConfigureAwait(false);

        await using var _ = request.ConfigureAwait(false);

        var returnedRequestObjectPath = await _instance.GetUserInformationAsync(
            window: _connectionManager.GetWindowIdentifier(windowIdentifier),
            options: options.ToVarDict()
        ).ConfigureAwait(false);

        await request.UpdateAsync(returnedRequestObjectPath).ConfigureAwait(false);
        return await request.GetTask().ConfigureAwait(false);
    }
}
