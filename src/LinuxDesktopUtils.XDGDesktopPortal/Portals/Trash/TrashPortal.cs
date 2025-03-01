using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Tmds.DBus.SourceGenerator;

namespace LinuxDesktopUtils.XDGDesktopPortal;

/// <summary>
/// Portal for trashing files.
///
/// This simple interface lets sandboxed applications send files to the trashcan.
/// </summary>
/// <remarks>
/// https://flatpak.github.io/xdg-desktop-portal/docs/doc-org.freedesktop.portal.Trash.html
/// </remarks>
[PublicAPI]
public class TrashPortal : IPortal
{
    private readonly DesktopPortalConnectionManager _connectionManager;
    private readonly OrgFreedesktopPortalTrashProxy  _instance;
    private readonly uint _version;

    /// <inheritdoc/>
    uint IPortal.Version => _version;

    private TrashPortal(
        DesktopPortalConnectionManager connectionManager,
        OrgFreedesktopPortalTrashProxy instance,
        uint version)
    {
        _connectionManager = connectionManager;
        _instance = instance;
        _version = version;
    }

    internal static async ValueTask<TrashPortal> CreateAsync(DesktopPortalConnectionManager connectionManager)
    {
        var instance = new OrgFreedesktopPortalTrashProxy(connectionManager.GetConnection(), destination: DBusHelper.BusName, path: DBusHelper.ObjectPath);
        var version = await instance.GetVersionPropertyAsync().ConfigureAwait(false);

        return new TrashPortal(connectionManager, instance, version);
    }

    /// <summary>
    /// Sends a file to the trashcan. Applications are allowed to trash a file if they can open it in r/w mode.
    /// </summary>
    /// <param name="file">Absolute path to the file.</param>
    /// <returns>Whether the file was trashed successfully</returns>
    public async Task<bool> TrashFileAsync(FilePath file)
    {
        using var safeFileHandle = File.OpenHandle(file.Value, FileMode.Open, FileAccess.ReadWrite);

        var result = await _instance.TrashFileAsync(
            fd: safeFileHandle
        ).ConfigureAwait(false);

        return result == 1;
    }
}
