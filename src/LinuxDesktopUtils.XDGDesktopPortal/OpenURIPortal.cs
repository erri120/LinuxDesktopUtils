using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Tmds.DBus.SourceGenerator;

namespace LinuxDesktopUtils.XDGDesktopPortal;

/// <summary>
/// Portal for opening URIs.
///
/// The OpenURI portal allows sandboxed applications to open URIs (e.g. a http: link to the applications homepage) under the control of the user.
/// </summary>
/// <remarks>
/// https://flatpak.github.io/xdg-desktop-portal/docs/doc-org.freedesktop.portal.OpenURI.html
/// </remarks>
[PublicAPI]
public class OpenUriPortal : IPortal
{
    private readonly DesktopPortalConnectionManager _connectionManager;
    private readonly OrgFreedesktopPortalOpenURI _instance;

    /// <inheritdoc/>
    public uint Version { get; }

    private OpenUriPortal(
        DesktopPortalConnectionManager connectionManager,
        OrgFreedesktopPortalOpenURI instance,
        uint version)
    {
        _connectionManager = connectionManager;
        _instance = instance;
        Version = version;
    }

    internal static async ValueTask<OpenUriPortal> CreateAsync(DesktopPortalConnectionManager connectionManager)
    {
        var instance = new OrgFreedesktopPortalOpenURI(connectionManager.GetConnection(), destination: DBusHelper.BusName, path: DBusHelper.ObjectPath);
        var version = await instance.GetVersionPropertyAsync().ConfigureAwait(false);

        return new OpenUriPortal(connectionManager, instance, version);
    }

    /// <summary>
    /// Asks to open a URI.
    /// </summary>
    /// <param name="uri">The Uri to open.</param>
    /// <param name="windowIdentifier">Identifier of the parent window.</param>
    /// <param name="options">Additional options.</param>
    /// <param name="cancellationToken">CancellationToken to cancel the request.</param>
    public async Task<Response> OpenUriAsync(
        Uri uri,
        Optional<WindowIdentifier> windowIdentifier = default,
        OpenUriOptions? options = null,
        Optional<CancellationToken> cancellationToken = default)
    {
        if (uri.Scheme.Equals("file", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException($"URIs with the `file` scheme are explicitly not supported by this method. Use {nameof(OpenFileAsync)} instead.", nameof(uri));

        const uint addedInVersion = 1;
        PortalVersionException.ThrowIf(requiredVersion: addedInVersion, availableVersion: Version);
        if (cancellationToken.HasValue) cancellationToken.Value.ThrowIfCancellationRequested();

        options ??= new OpenUriOptions();

        var request = await _connectionManager.CreateRequestAsync(options.HandleToken, cancellationToken).ConfigureAwait(false);
        await using var _ = request.ConfigureAwait(false);

        var returnedRequestObjectPath = await _instance.OpenURIAsync(
            parentWindow: _connectionManager.GetWindowIdentifier(windowIdentifier),
            uri: uri.ToString(),
            options: options.ToVarDict()
        ).ConfigureAwait(false);

        await request.UpdateAsync(returnedRequestObjectPath).ConfigureAwait(false);
        return await request.GetTask().ConfigureAwait(false);
    }

    /// <summary>
    /// Asks to open a local file.
    /// </summary>
    /// <param name="file">Absolute path to a local file.</param>
    /// <param name="windowIdentifier">Identifier of the parent window.</param>
    /// <param name="options">Additional options.</param>
    /// <param name="cancellationToken">CancellationToken to cancel the request.</param>
    public async Task<Response> OpenFileAsync(
        FilePath file,
        Optional<WindowIdentifier> windowIdentifier = default,
        OpenFileOptions? options = null,
        Optional<CancellationToken> cancellationToken = default)
    {
        const uint addedInVersion = 2;
        PortalVersionException.ThrowIf(requiredVersion: addedInVersion, availableVersion: Version);
        if (cancellationToken.HasValue) cancellationToken.Value.ThrowIfCancellationRequested();

        using var safeFileHandle = File.OpenHandle(file.Value);

        options ??= new OpenFileOptions();

        var request = await _connectionManager.CreateRequestAsync(options.HandleToken, cancellationToken).ConfigureAwait(false);
        await using var _ = request.ConfigureAwait(false);

        var returnedRequestObjectPath = await _instance.OpenFileAsync(
            parentWindow: _connectionManager.GetWindowIdentifier(windowIdentifier),
            fd: safeFileHandle,
            options: options.ToVarDict()
        ).ConfigureAwait(false);

        await request.UpdateAsync(returnedRequestObjectPath).ConfigureAwait(false);
        return await request.GetTask().ConfigureAwait(false);
    }

    /// <summary>
    /// Asks to open the directory containing a local file in the file browser.
    /// </summary>
    /// <param name="file">Absolute path to a local file.</param>
    /// <param name="windowIdentifier">Identifier of the parent window.</param>
    /// <param name="options">Additional options.</param>
    /// <param name="cancellationToken">CancellationToken to cancel the request.</param>
    public async Task<Response> OpenFileInDirectoryAsync(
        FilePath file,
        Optional<WindowIdentifier> windowIdentifier = default,
        OpenFileInDirectoryOptions? options = null,
        Optional<CancellationToken> cancellationToken = default)
    {
        const uint addedInVersion = 3;
        PortalVersionException.ThrowIf(requiredVersion: addedInVersion, availableVersion: Version);
        if (cancellationToken.HasValue) cancellationToken.Value.ThrowIfCancellationRequested();

        using var safeFileHandle = File.OpenHandle(file.Value);

        options ??= new OpenFileInDirectoryOptions();

        var request = await _connectionManager.CreateRequestAsync(options.HandleToken, cancellationToken).ConfigureAwait(false);
        await using var _ = request.ConfigureAwait(false);

        var returnedRequestObjectPath = await _instance.OpenDirectoryAsync(
            parentWindow: _connectionManager.GetWindowIdentifier(windowIdentifier),
            fd: safeFileHandle,
            options: options.ToVarDict()
        ).ConfigureAwait(false);

        await request.UpdateAsync(returnedRequestObjectPath).ConfigureAwait(false);
        return await request.GetTask().ConfigureAwait(false);
    }
}
