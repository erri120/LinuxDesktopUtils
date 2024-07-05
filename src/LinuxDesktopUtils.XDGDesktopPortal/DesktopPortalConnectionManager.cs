using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Tmds.DBus.Protocol;

namespace LinuxDesktopUtils.XDGDesktopPortal;

/// <summary>
/// Connection manager. This is what you'll be interacting with the most.
/// </summary>
[PublicAPI]
public sealed class DesktopPortalConnectionManager : IAsyncDisposable
{
    private readonly CancellationTokenSource _cts = new();
    private readonly ConcurrentDictionary<Type, IPortal> _portalInstances = new();

    private readonly Connection _connection;
    private readonly string _uniqueName;
    private readonly string _senderName;
    private readonly Optional<WindowIdentifier> _defaultWindowIdentifier;

    private bool _isDisposed;

    internal DesktopPortalConnectionManager(Connection connection, Optional<WindowIdentifier> defaultWindowIdentifier)
    {
        _connection = connection;
        _uniqueName = connection.UniqueName ?? throw new ArgumentNullException(nameof(connection), "Connection doesn't have a unique name");
        _senderName = DBusHelper.UniqueNameToSenderName(_uniqueName);
        _defaultWindowIdentifier = defaultWindowIdentifier;
    }

    internal Connection GetConnection()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return _connection;
    }

    internal string GetWindowIdentifier(Optional<WindowIdentifier> preferredIdentifier)
    {
        if (preferredIdentifier.HasValue) return preferredIdentifier.Value.ToString();
        if (_defaultWindowIdentifier.HasValue) return _defaultWindowIdentifier.Value.ToString();
        return string.Empty;
    }

    /// <summary>
    /// Gets the <see cref="OpenUriPortal"/>.
    /// </summary>
    public ValueTask<OpenUriPortal> GetOpenUriPortalAsync()
    {
        if (!_portalInstances.TryGetValue(typeof(OpenUriPortal), out var portal)) return GetOpenUriPortalImplAsync();
        Debug.Assert(portal is OpenUriPortal);
        return new ValueTask<OpenUriPortal>((portal as OpenUriPortal)!);
    }

    private async ValueTask<OpenUriPortal> GetOpenUriPortalImplAsync()
    {
        var portal = await OpenUriPortal.CreateAsync(this).ConfigureAwait(false);
        var res = _portalInstances.GetOrAdd(typeof(OpenUriPortal), portal);
        Debug.Assert(res is OpenUriPortal);
        return (res as OpenUriPortal)!;
    }

    internal ValueTask<RequestWrapper> CreateRequestAsync(string handleToken, Optional<CancellationToken> customCancellationToken)
    {
        var cts = customCancellationToken.HasValue
            ? CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, customCancellationToken.Value)
            : _cts;

        ObjectDisposedException.ThrowIf(_isDisposed, this);
        var requestObjectPath = new ObjectPath($"{DBusHelper.ObjectPath}/request/{_senderName}/{handleToken}");

        return RequestWrapper.CreateAsync(
            connectionManager: this,
            requestObjectPath,
            cancellationToken: cts.Token
        );
    }

    /// <summary>
    /// Cancels all pending requests, disconnects from the D-Bus, and disposes all resources.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        try
        {
            await _cts.CancelAsync().ConfigureAwait(false);
            _cts.Dispose();

            // NOTE(erri120): DisconnectedAsync returns an empty task, the result of the task is set by Dispose
            _connection.Dispose();
            await _connection.DisconnectedAsync().ConfigureAwait(false);
        }
        catch (Exception)
        {
            // ignored
        }
        finally
        {
            _isDisposed = true;
        }
    }

    /// <summary>
    /// Connects to the D-Bus at <paramref name="address"/>.
    /// </summary>
    /// <param name="defaultWindowIdentifier">Default window identifier. This is useful if you only have one window and don't want to or can't provide a window identifier everywhere.</param>
    /// <param name="address">Address to connect to. If this isn't specified, it'll default to <see cref="Address.Session"/>.</param>
    public static async ValueTask<DesktopPortalConnectionManager> ConnectAsync(
        Optional<WindowIdentifier> defaultWindowIdentifier = default,
        Optional<string> address = default)
    {
        var addressValue = address.HasValue ? address.Value : Address.Session;
        if (addressValue is null) throw new Exception("Address is null!");

        var connection = new Connection(new ClientConnectionOptions(addressValue)
        {
            AutoConnect = false,
        });

        await connection.ConnectAsync().ConfigureAwait(false);
        return new DesktopPortalConnectionManager(connection, defaultWindowIdentifier);
    }
}
