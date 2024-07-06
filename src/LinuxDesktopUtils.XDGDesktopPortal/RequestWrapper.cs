using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tmds.DBus.Protocol;
using Tmds.DBus.SourceGenerator;

namespace LinuxDesktopUtils.XDGDesktopPortal;

internal class RequestWrapper<T> : ARequestWrapper<Response<T>>
where T : notnull
{
    internal delegate T ResultsDelegate(Dictionary<string, VariantValue> varDict);

    private readonly ResultsDelegate _resultsDelegate;

    private RequestWrapper(
        DesktopPortalConnectionManager connectionManager,
        TaskCompletionSource<Response<T>> tsc,
        OrgFreedesktopPortalRequest request,
        ObjectPath requestObjectPath,
        IDisposable subscriptionDisposable,
        CancellationToken cancellationToken,
        ResultsDelegate resultsDelegate) : base(connectionManager, tsc, request, requestObjectPath, subscriptionDisposable, cancellationToken)
    {
        _resultsDelegate = resultsDelegate;
    }

    protected override ValueTask<IDisposable> CreateSubscription(OrgFreedesktopPortalRequest request, TaskCompletionSource<Response<T>> tsc)
    {
        return CreateSubscriptionImpl(request, tsc, _resultsDelegate);
    }

    private static async ValueTask<IDisposable> CreateSubscriptionImpl(
        OrgFreedesktopPortalRequest request,
        TaskCompletionSource<Response<T>> tsc,
        ResultsDelegate resultsDelegate)
    {
        return await request.WatchResponseAsync((exception, resultTuple) =>
        {
            if (exception is not null) tsc.TrySetException(exception);
            else
            {
                var responseStatus = (ResponseStatus)resultTuple.Response;
                tsc.TrySetResult(new Response<T>
                {
                    Status = responseStatus,
                    Results = resultsDelegate(resultTuple.Results),
                });
            }
        }, emitOnCapturedContext: false).ConfigureAwait(false);
    }

    internal static async ValueTask<RequestWrapper<T>> CreateAsync(
        DesktopPortalConnectionManager connectionManager,
        ObjectPath requestObjectPath,
        CancellationToken cancellationToken,
        ResultsDelegate resultsDelegate)
    {
        var request = CreateRequest(connectionManager, requestObjectPath);

        var tsc = new TaskCompletionSource<Response<T>>();
        var disposable = await CreateSubscriptionImpl(request, tsc, resultsDelegate).ConfigureAwait(false);

        return new RequestWrapper<T>(connectionManager, tsc, request, requestObjectPath, disposable, cancellationToken, resultsDelegate);
    }
}

internal class RequestWrapper : ARequestWrapper<Response>
{
    private RequestWrapper(
        DesktopPortalConnectionManager connectionManager,
        TaskCompletionSource<Response> tsc,
        OrgFreedesktopPortalRequest request,
        ObjectPath requestObjectPath,
        IDisposable subscriptionDisposable,
        CancellationToken cancellationToken) : base(connectionManager, tsc, request, requestObjectPath, subscriptionDisposable, cancellationToken) { }

    protected override ValueTask<IDisposable> CreateSubscription(OrgFreedesktopPortalRequest request, TaskCompletionSource<Response> tsc)
    {
        return CreateSubscriptionImpl(request, tsc);
    }

    private static async ValueTask<IDisposable> CreateSubscriptionImpl(
        OrgFreedesktopPortalRequest request,
        TaskCompletionSource<Response> tsc)
    {
        return await request.WatchResponseAsync((exception, resultTuple) =>
        {
            if (exception is not null) tsc.TrySetException(exception);
            else
            {
                var responseStatus = (ResponseStatus)resultTuple.Response;
                tsc.TrySetResult(new Response
                {
                    Status = responseStatus,
                });
            }
        }, emitOnCapturedContext: false).ConfigureAwait(false);
    }

    internal static async ValueTask<RequestWrapper> CreateAsync(
        DesktopPortalConnectionManager connectionManager,
        ObjectPath requestObjectPath,
        CancellationToken cancellationToken)
    {
        var request = CreateRequest(connectionManager, requestObjectPath);

        var tsc = new TaskCompletionSource<Response>();
        var disposable = await CreateSubscriptionImpl(request, tsc).ConfigureAwait(false);

        return new RequestWrapper(connectionManager, tsc, request, requestObjectPath, disposable, cancellationToken);
    }
}

internal abstract class ARequestWrapper<T> : IAsyncDisposable
where T : notnull
{
    private readonly DesktopPortalConnectionManager _connectionManager;
    private readonly TaskCompletionSource<T> _tsc;
    private readonly CancellationTokenRegistration _cancellationTokenRegistration;

    private OrgFreedesktopPortalRequest _request;
    private ObjectPath _requestObjectPath;
    private IDisposable _subscriptionDisposable;
    private bool _isDisposed;

    protected ARequestWrapper(
        DesktopPortalConnectionManager connectionManager,
        TaskCompletionSource<T> tsc,
        OrgFreedesktopPortalRequest request,
        ObjectPath requestObjectPath,
        IDisposable subscriptionDisposable,
        CancellationToken cancellationToken)
    {
        _connectionManager = connectionManager;
        _tsc = tsc;
        _request = request;
        _requestObjectPath = requestObjectPath;
        _subscriptionDisposable = subscriptionDisposable;

        _cancellationTokenRegistration = cancellationToken.Register(CancelCallback, this);
    }

    internal Task<T> GetTask()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return _tsc.Task;
    }

    internal ValueTask UpdateAsync(ObjectPath returnedRequestObjectPath)
    {
        // https://flatpak.github.io/xdg-desktop-portal/docs/doc-org.freedesktop.portal.Request.html
        // It is recommended that the caller should verify that the returned handle is what it expected, and update its signal subscription if it isn’t.
        // This ensures that applications will work with both old and new versions of xdg-desktop-portal.

        if (returnedRequestObjectPath.ToString().Equals(_requestObjectPath.ToString(), StringComparison.OrdinalIgnoreCase)) return ValueTask.CompletedTask;
        return UpdateImplAsync(returnedRequestObjectPath);
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

        var request = CreateRequest(_connectionManager, returnedRequestObjectPath);
        var disposable = await CreateSubscription(request, _tsc).ConfigureAwait(false);

        _request = request;
        _requestObjectPath = returnedRequestObjectPath;
        _subscriptionDisposable = disposable;
    }

    protected abstract ValueTask<IDisposable> CreateSubscription(OrgFreedesktopPortalRequest request, TaskCompletionSource<T> tsc);

    private static void CancelCallback(object? state, CancellationToken cancellationToken)
    {
        if (state is null) return;
        if (state is not ARequestWrapper<T> request)
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

    protected static OrgFreedesktopPortalRequest CreateRequest(
        DesktopPortalConnectionManager connectionManager,
        ObjectPath requestObjectPath)
    {
        return new OrgFreedesktopPortalRequest(
            connectionManager.GetConnection(),
            destination: DBusHelper.BusName,
            path: requestObjectPath.ToString()
        );
    }
}
