using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Tmds.DBus.Protocol;
using Tmds.DBus.SourceGenerator;

namespace LinuxDesktopUtils.XDGDesktopPortal;

/// <summary>
/// The Secret portal allows sandboxed applications to retrieve a per-application secret.
/// The secret can then be used for encrypting confidential data inside the sandbox.
/// </summary>
/// <remarks>
/// https://flatpak.github.io/xdg-desktop-portal/docs/doc-org.freedesktop.portal.Secret.html
/// </remarks>
[PublicAPI]
public partial class SecretPortal : IPortal
{
    private readonly DesktopPortalConnectionManager _connectionManager;
    private readonly OrgFreedesktopPortalSecret _instance;
    private readonly uint _version;

    /// <inheritdoc/>
    uint IPortal.Version => _version;

    private SecretPortal(DesktopPortalConnectionManager connectionManager, OrgFreedesktopPortalSecret instance, uint version)
    {
        _connectionManager = connectionManager;
        _instance = instance;
        _version = version;
    }

    internal static async ValueTask<SecretPortal> CreateAsync(DesktopPortalConnectionManager connectionManager)
    {
        var instance = new OrgFreedesktopPortalSecret(connectionManager.GetConnection(), destination: DBusHelper.BusName, path: DBusHelper.ObjectPath);
        var version = await instance.GetVersionPropertyAsync().ConfigureAwait(false);

        return new SecretPortal(connectionManager, instance, version);
    }

    /// <summary>
    /// Retrieves a master secret for a sandboxed application.
    /// </summary>
    /// <remarks>
    /// The master secret is unique per application and does not change as long as the application is installed (once it has been created).
    /// In a typical backend implementation, it is stored in the user’s keyring, under the application ID as a key.
    ///
    /// While the master secret can be used for encrypting any confidential data in the sandbox, the format is opaque to the application.
    /// In particular, the length of the secret might not be sufficient for the use with certain encryption algorithm.
    /// In that case, the application is supposed to expand it using a KDF algorithm.
    /// </remarks>
    /// <param name="cancellationToken">CancellationToken to cancel the request.</param>
    /// <exception cref="PortalVersionException">Thrown if the installed portal backend doesn't support this method.</exception>
    public async Task<Response<RetrieveSecretResult>> RetrieveSecretAsync(Optional<CancellationToken> cancellationToken = default)
    {
        const uint addedInVersion = 1;
        PortalVersionException.ThrowIf(requiredVersion: addedInVersion, availableVersion: _version);
        if (cancellationToken.HasValue) cancellationToken.Value.ThrowIfCancellationRequested();

        var handleToken = DBusHelper.CreateHandleToken();

        var request = await _connectionManager.CreateRequestAsync(
            handleToken,
            cancellationToken
        ).ConfigureAwait(false);

        await using var _ = request.ConfigureAwait(false);

        using var tmpFile = TempFile.New();
        var returnedRequestObjectPath = await _instance.RetrieveSecretAsync(
            fd: File.OpenHandle(tmpFile.Value, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite),
            options: new Dictionary<string, Variant>(StringComparer.Ordinal)
            {
                { "handle_token", handleToken },
            }
        ).ConfigureAwait(false);

        await request.UpdateAsync(returnedRequestObjectPath).ConfigureAwait(false);
        var response = await request.GetTask().ConfigureAwait(false);

        if (response.Status != ResponseStatus.Success)
        {
            return new Response<RetrieveSecretResult>
            {
                Status = response.Status,
                Results = Optional<RetrieveSecretResult>.None,
            };
        }

        var bytes = await File.ReadAllBytesAsync(tmpFile.Value, cancellationToken: request.GetCancellationToken()).ConfigureAwait(false);
        return new Response<RetrieveSecretResult>
        {
            Status = response.Status,
            Results = new RetrieveSecretResult
            {
                Secret = bytes,
            },
        };
    }
}
