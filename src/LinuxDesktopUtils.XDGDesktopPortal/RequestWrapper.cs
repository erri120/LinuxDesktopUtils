using System;
using System.Threading;
using System.Threading.Tasks;
using Tmds.DBus.Protocol;
using Tmds.DBus.SourceGenerator;

namespace LinuxDesktopUtils.XDGDesktopPortal;

internal static class RequestWrapper
{
    internal static async Task<Response> Create(Connection connection, ObjectPath objectPath, CancellationToken cancellationToken)
    {
        var request = new OrgFreedesktopPortalRequest(connection, DBusHelper.BusName, objectPath.ToString());
        await using var requestCancellationTokenRegistration = cancellationToken.Register(CancelRequest, request).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();
        var tsc = new TaskCompletionSource<Response>();

        using var disposable = await request.WatchResponseAsync((exception, tuple) =>
        {
            if (exception is not null) tsc.TrySetException(exception);
            else tsc.TrySetResult((Response)tuple.Response);
        }).ConfigureAwait(false);

        await using var disposableCancellationTokenRegistration = cancellationToken.Register(DisposeDisposable, disposable).ConfigureAwait(false);
        await using var taskCancellationTokenRegistration = cancellationToken.Register(CancelTaskCompletionSource, tsc).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();

        return await tsc.Task.ConfigureAwait(false);
    }

    private static void CancelTaskCompletionSource(object? state, CancellationToken cancellationToken)
    {
        if (state is null) return;
        if (state is not TaskCompletionSource<Response> tsc)
            throw new ArgumentException($"Invalid argument: {state.GetType()}", nameof(state));

        tsc.TrySetCanceled(cancellationToken);
    }

    private static void DisposeDisposable(object? state)
    {
        if (state is null) return;
        if (state is not IDisposable disposable)
            throw new ArgumentException($"Invalid argument: {state.GetType()}", nameof(state));

        disposable.Dispose();
    }

    private static void CancelRequest(object? state)
    {
        if (state is null) return;
        if (state is not OrgFreedesktopPortalRequest request)
            throw new ArgumentException($"Invalid argument: {state.GetType()}", nameof(state));

        _ = request.CloseAsync();
    }
}
