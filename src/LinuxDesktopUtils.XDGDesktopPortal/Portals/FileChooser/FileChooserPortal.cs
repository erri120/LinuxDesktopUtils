using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Tmds.DBus.SourceGenerator;

namespace LinuxDesktopUtils.XDGDesktopPortal;

/// <summary>
/// The FileChooser portal allows sandboxed applications to ask the user for access to files outside the sandbox.
/// The portal backend will present the user with a file chooser dialog.
///
/// The selected files will be made accessible to the application (which may involve adding it to the Documents portal).
/// </summary>
/// <remarks>
/// https://flatpak.github.io/xdg-desktop-portal/docs/doc-org.freedesktop.portal.FileChooser.html
/// </remarks>
[PublicAPI]
public partial class FileChooserPortal : IPortal
{
    private readonly DesktopPortalConnectionManager _connectionManager;
    private readonly OrgFreedesktopPortalFileChooserProxy _instance;
    private readonly uint _version;

    /// <inheritdoc/>
    uint IPortal.Version => _version;

    private FileChooserPortal(DesktopPortalConnectionManager connectionManager, OrgFreedesktopPortalFileChooserProxy instance, uint version)
    {
        _connectionManager = connectionManager;
        _instance = instance;
        _version = version;
    }

    internal static async ValueTask<FileChooserPortal> CreateAsync(DesktopPortalConnectionManager connectionManager)
    {
        var instance = new OrgFreedesktopPortalFileChooserProxy(connectionManager.GetConnection(), destination: DBusHelper.BusName, path: DBusHelper.ObjectPath);
        var version = await instance.GetVersionPropertyAsync().ConfigureAwait(false);

        return new FileChooserPortal(connectionManager, instance, version);
    }

    /// <summary>
    /// Asks to open one or more files.
    /// </summary>
    /// <param name="dialogTitle">Title for the file chooser dialog.</param>
    /// <param name="windowIdentifier">Identifier of the parent window.</param>
    /// <param name="options">Additional options.</param>
    /// <param name="cancellationToken">CancellationToken to cancel the request.</param>
    /// <exception cref="PortalVersionException">Thrown if the installed portal backend doesn't support this method.</exception>
    public async Task<Response<OpenFileResults>> OpenFileAsync(
        string dialogTitle,
        Optional<WindowIdentifier> windowIdentifier = default,
        OpenFileOptions? options = null,
        Optional<CancellationToken> cancellationToken = default)
    {
        const uint addedInVersion = 1;
        PortalVersionException.ThrowIf(requiredVersion: addedInVersion, availableVersion: _version);
        if (cancellationToken.HasValue) cancellationToken.Value.ThrowIfCancellationRequested();

        options ??= new OpenFileOptions();

        var request = await _connectionManager.CreateRequestAsync(
            options.HandleToken,
            resultsDelegate: varDict => OpenFileResults.From(options, varDict),
            cancellationToken
        ).ConfigureAwait(false);

        await using var _ = request.ConfigureAwait(false);

        var returnedRequestObjectPath = await _instance.OpenFileAsync(
            parentWindow: _connectionManager.GetWindowIdentifier(windowIdentifier),
            title: dialogTitle,
            options: options.ToVarDict()
        ).ConfigureAwait(false);

        await request.UpdateAsync(returnedRequestObjectPath).ConfigureAwait(false);
        return await request.GetTask().ConfigureAwait(false);
    }

    /// <summary>
    /// Asks for a location to save a file.
    /// </summary>
    /// <param name="dialogTitle">Title for the file chooser dialog.</param>
    /// <param name="windowIdentifier">Identifier of the parent window.</param>
    /// <param name="options">Additional options.</param>
    /// <param name="cancellationToken">CancellationToken to cancel the request.</param>
    /// <exception cref="PortalVersionException">Thrown if the installed portal backend doesn't support this method.</exception>
    public async Task<Response<SaveFileResults>> SaveFileAsync(
        string dialogTitle,
        Optional<WindowIdentifier> windowIdentifier = default,
        SaveFileOptions? options = null,
        Optional<CancellationToken> cancellationToken = default)
    {
        const uint addedInVersion = 1;
        PortalVersionException.ThrowIf(requiredVersion: addedInVersion, availableVersion: _version);
        if (cancellationToken.HasValue) cancellationToken.Value.ThrowIfCancellationRequested();

        options ??= new SaveFileOptions();

        var request = await _connectionManager.CreateRequestAsync(
            options.HandleToken,
            resultsDelegate: varDict => SaveFileResults.From(options, varDict),
            cancellationToken
        ).ConfigureAwait(false);

        await using var _ = request.ConfigureAwait(false);

        var returnedRequestObjectPath = await _instance.SaveFileAsync(
            parentWindow: _connectionManager.GetWindowIdentifier(windowIdentifier),
            title: dialogTitle,
            options: options.ToVarDict()
        ).ConfigureAwait(false);

        await request.UpdateAsync(returnedRequestObjectPath).ConfigureAwait(false);
        return await request.GetTask().ConfigureAwait(false);
    }
}
