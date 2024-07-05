using System;
using System.Threading;
using System.Threading.Tasks;
using Tmds.DBus.Protocol;
using Tmds.DBus.SourceGenerator;

namespace LinuxDesktopUtils.XDGDesktopPortal;

internal class RequestWrapper : IAsyncDisposable
{
    private readonly DesktopPortalConnectionManager _connectionManager;
    private readonly TaskCompletionSource<Response> _tsc;
    private readonly CancellationTokenRegistration _cancellationTokenRegistration;

    private OrgFreedesktopPortalRequest _request;
    private ObjectPath _requestObjectPath;
    private IDisposable _subscriptionDisposable;
    private bool _isDisposed;

    private RequestWrapper(
        DesktopPortalConnectionManager connectionManager,
        OrgFreedesktopPortalRequest request,
        ObjectPath requestObjectPath,
        TaskCompletionSource<Response> tsc,
        IDisposable subscriptionDisposable,
        CancellationToken cancellationToken)
    {
        _connectionManager = connectionManager;
        _request = request;
        _requestObjectPath = requestObjectPath;
        _tsc = tsc;
        _subscriptionDisposable = subscriptionDisposable;
        _cancellationTokenRegistration = cancellationToken.Register(CancelCallback, this);
    }

    internal ValueTask UpdateAsync(ObjectPath returnedRequestObjectPath)
    {
        // https://flatpak.github.io/xdg-desktop-portal/docs/doc-org.freedesktop.portal.Request.html
        // It is recommended that the caller should verify that the returned handle is what it expected, and update its signal subscription if it isn’t.
        // This ensures that applications will work with both old and new versions of xdg-desktop-portal.

        if (returnedRequestObjectPath.ToString().Equals(_requestObjectPath.ToString(), StringComparison.OrdinalIgnoreCase)) return ValueTask.CompletedTask;
        return UpdateImplAsync(returnedRequestObjectPath);
    }

    internal static async ValueTask<RequestWrapper> CreateAsync(
        DesktopPortalConnectionManager connectionManager,
        ObjectPath requestObjectPath,
        CancellationToken cancellationToken)
    {
        var request = new OrgFreedesktopPortalRequest(
            connectionManager.GetConnection(),
            destination: DBusHelper.BusName,
            path: requestObjectPath.ToString()
        );

        var tsc = new TaskCompletionSource<Response>();
        var disposable = await request.WatchResponseAsync((exception, resultTuple) =>
        {
            if (exception is not null) tsc.TrySetException(exception);
            else tsc.TrySetResult((Response)resultTuple.Response);
        }, emitOnCapturedContext: false).ConfigureAwait(false);

        return new RequestWrapper(connectionManager, request, requestObjectPath, tsc, disposable, cancellationToken);
    }

    private async ValueTask UpdateImplAsync(ObjectPath returnedRequestObjectPath)
    {
        try
        {
            _subscriptionDisposable.Dispose();
        }
        catch (Exception)
        {
            // ignored
        }

        var request = new OrgFreedesktopPortalRequest(
            _connectionManager.GetConnection(),
            destination: DBusHelper.BusName,
            path: returnedRequestObjectPath
        );

        var disposable = await request.WatchResponseAsync((exception, resultTuple) =>
        {
            if (exception is not null) _tsc.TrySetException(exception);
            else _tsc.TrySetResult((Response)resultTuple.Response);
        }, emitOnCapturedContext: false).ConfigureAwait(false);

        _request = request;
        _requestObjectPath = returnedRequestObjectPath;
        _subscriptionDisposable = disposable;
    }

    internal Task<Response> GetTask()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return _tsc.Task;
    }

    private static void CancelCallback(object? state, CancellationToken cancellationToken)
    {
        if (state is null) return;
        if (state is not RequestWrapper request)
            throw new ArgumentException($"Invalid argument: {state.GetType()}, expected {typeof(RequestWrapper)}", nameof(state));

        request._tsc.TrySetCanceled(cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        try
        {
            _subscriptionDisposable.Dispose();

            if (!_tsc.Task.IsCompleted)
            {
                _tsc.TrySetCanceled(CancellationToken.None);

                // NOTE(erri120): If the task completed, then the request doesn't exist anymore and can't be closed
                await _request.CloseAsync().ConfigureAwait(false);
            }

            await _cancellationTokenRegistration.DisposeAsync().ConfigureAwait(false);
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

}
