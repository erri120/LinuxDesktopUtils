using System;
using System.Collections.Generic;
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
    private readonly Dictionary<Type, IPortal> _portalInstances = new();
    private readonly SemaphoreSlim _instancesSemaphore = new(initialCount: 1, maxCount: 1);

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
    /// Gets the <see cref="AccountPortal"/>.
    /// </summary>
    /// <returns></returns>
    public ValueTask<AccountPortal> GetAccountPortalAsync() => GetPortalAsync(AccountPortal.CreateAsync);

    /// <summary>
    /// Gets the <see cref="FileChooserPortal"/>.
    /// </summary>
    public ValueTask<FileChooserPortal> GetFileChooserPortalAsync() => GetPortalAsync(FileChooserPortal.CreateAsync);

    /// <summary>
    /// Gets the <see cref="NetworkMonitorPortal"/>.
    /// </summary>
    /// <returns></returns>
    public ValueTask<NetworkMonitorPortal> GetNetworkMonitorPortalAsync() => GetPortalAsync(NetworkMonitorPortal.CreateAsync);

    /// <summary>
    /// Gets the <see cref="OpenUriPortal"/>.
    /// </summary>
    public ValueTask<OpenUriPortal> GetOpenUriPortalAsync() => GetPortalAsync(OpenUriPortal.CreateAsync);

    /// <summary>
    /// Gets the <see cref="SecretPortal"/>.
    /// </summary>
    public ValueTask<SecretPortal> GetSecretPortalAsync() => GetPortalAsync(SecretPortal.CreateAsync);

    /// <summary>
    /// Gets the <see cref="TrashPortal"/>.
    /// </summary>
    public ValueTask<TrashPortal> GetTrashPortalAsync() => GetPortalAsync(TrashPortal.CreateAsync);

    private ValueTask<T> GetPortalAsync<T>(Func<DesktopPortalConnectionManager, ValueTask<T>> factory) where T : class, IPortal
    {
        if (!_portalInstances.TryGetValue(typeof(T), out var portal)) return GetPortalImplAsync(factory);
        Debug.Assert(portal is T);
        return ValueTask.FromResult((portal as T)!);
    }

    private async ValueTask<T> GetPortalImplAsync<T>(Func<DesktopPortalConnectionManager, ValueTask<T>> factory) where T : class, IPortal
    {
        using var _ = await _instancesSemaphore.WaitDisposableAsync(_cts.Token).ConfigureAwait(false);
        if (_portalInstances.TryGetValue(typeof(T), out var portal))
        {
            Debug.Assert(portal is T);
            return (portal as T)!;
        }

        try
        {
            portal = await factory(this).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            throw new PortalUnavailableException(typeof(T), e);
        }

        Debug.Assert(portal is T);
        _portalInstances[typeof(T)] = portal;
        return (portal as T)!;
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

    internal ValueTask<RequestWrapper<T>> CreateRequestAsync<T>(
        string handleToken,
        RequestWrapper<T>.ResultsDelegate resultsDelegate,
        Optional<CancellationToken> customCancellationToken) where T : notnull
    {
        var cts = customCancellationToken.HasValue
            ? CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, customCancellationToken.Value)
            : _cts;

        ObjectDisposedException.ThrowIf(_isDisposed, this);
        var requestObjectPath = new ObjectPath($"{DBusHelper.ObjectPath}/request/{_senderName}/{handleToken}");

        return RequestWrapper<T>.CreateAsync(
            connectionManager: this,
            requestObjectPath,
            cancellationToken: cts.Token,
            resultsDelegate
        );
    }

    /// <summary>
    /// Cancels all pending requests, disconnects from the D-Bus, and disposes all resources.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_isDisposed) return;

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
