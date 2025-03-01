using System;
using System.Threading;
using System.Threading.Tasks;

namespace LinuxDesktopUtils.XDGDesktopPortal;

internal readonly struct DisposableSemaphoreSlim : IDisposable
{
    private readonly SemaphoreSlim _semaphoreSlim;
    private readonly bool _hasEntered;

    internal DisposableSemaphoreSlim(SemaphoreSlim semaphoreSlim, bool hasEntered)
    {
        _semaphoreSlim = semaphoreSlim;
        _hasEntered = hasEntered;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (!_hasEntered) return;
        _semaphoreSlim.Release();
    }
}

internal static class SemaphoreExtensions
{
    public static async ValueTask<DisposableSemaphoreSlim> WaitDisposableAsync(
        this SemaphoreSlim semaphoreSlim,
        CancellationToken cancellationToken = default)
    {
        await semaphoreSlim.WaitAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        return new DisposableSemaphoreSlim(semaphoreSlim, hasEntered: true);
    }
}
