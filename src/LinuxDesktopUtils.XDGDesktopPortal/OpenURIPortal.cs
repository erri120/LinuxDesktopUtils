using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Win32.SafeHandles;
using OneOf;
using Tmds.DBus.Protocol;
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
public static class OpenUriPortal
{
    /// <summary>
    /// Asks to open a URI.
    /// </summary>
    /// <param name="connection">DBus connection to use.</param>
    /// <param name="windowIdentifier">Identifier of the parent window.</param>
    /// <param name="uri">The Uri to open.</param>
    /// <param name="options">Additional options.</param>
    /// <param name="cancellationToken">CancellationToken to cancel the request.</param>
    public static async Task<Response> OpenUriAsync(
        Connection connection,
        WindowIdentifier windowIdentifier,
        Uri uri,
        OpenUriOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (uri.Scheme.Equals("file", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException($"URIs with the `file` scheme are explicitly not supported by this method. Use {nameof(OpenFileAsync)} instead.", nameof(uri));

        cancellationToken.ThrowIfCancellationRequested();
        var openUri = new OrgFreedesktopPortalOpenURI(connection, DBusHelper.BusName, DBusHelper.ObjectPath);

        var res = await openUri.OpenURIAsync(
            parentWindow: windowIdentifier.ToString(),
            uri: uri.ToString(),
            options: (options ?? OpenUriOptions.Default).ToVarDict()
        ).ConfigureAwait(false);

        return await RequestWrapper.Create(connection, res, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asks to open a local file.
    /// </summary>
    /// <param name="connection">DBus connection to use.</param>
    /// <param name="windowIdentifier">Identifier of the parent window.</param>
    /// <param name="file">Absolute path to the local file or an existing file handle.</param>
    /// <param name="options">Additional options.</param>
    /// <param name="cancellationToken">CancellationToken to cancel the request.</param>
    public static async Task<Response> OpenFileAsync(
        Connection connection,
        WindowIdentifier windowIdentifier,
        OneOf<FilePath, SafeFileHandle> file,
        OpenFileOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        using var safeFileHandle = file.IsT1 ? file.AsT1 : File.OpenHandle(file.AsT0.Value);

        cancellationToken.ThrowIfCancellationRequested();
        var openUri = new OrgFreedesktopPortalOpenURI(connection, DBusHelper.BusName, DBusHelper.ObjectPath);

        var res = await openUri.OpenFileAsync(
            parentWindow: windowIdentifier.ToString(),
            fd: safeFileHandle,
            options: (options ?? OpenFileOptions.Default).ToVarDict()
        ).ConfigureAwait(false);

        return await RequestWrapper.Create(connection, res, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asks to open the directory containing a local file in the file browser.
    /// </summary>
    /// <param name="connection">DBus connection to use.</param>
    /// <param name="windowIdentifier">Identifier of the parent window.</param>
    /// <param name="file">Absolute path to the local file or an existing file handle.</param>
    /// <param name="cancellationToken">CancellationToken to cancel the request.</param>
    public static async Task<Response> OpenFileInDirectoryAsync(
        Connection connection,
        WindowIdentifier windowIdentifier,
        OneOf<FilePath, SafeFileHandle> file,
        CancellationToken cancellationToken = default)
    {
        using var safeFileHandle = file.IsT1 ? file.AsT1 : File.OpenHandle(file.AsT0.Value);

        cancellationToken.ThrowIfCancellationRequested();
        var openUri = new OrgFreedesktopPortalOpenURI(connection, DBusHelper.BusName, DBusHelper.ObjectPath);

        var res = await openUri.OpenDirectoryAsync(
            parentWindow: windowIdentifier.ToString(),
            fd: safeFileHandle,
            options: DBusHelper.EmptyVarDict
        ).ConfigureAwait(false);

        return await RequestWrapper.Create(connection, res, cancellationToken).ConfigureAwait(false);
    }
}
