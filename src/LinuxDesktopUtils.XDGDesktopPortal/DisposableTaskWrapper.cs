using System;
using System.Threading.Tasks;

namespace LinuxDesktopUtils.XDGDesktopPortal;

internal class DisposableTaskWrapper : IDisposable
{
    private IDisposable? _disposable;
    private readonly Task _task;

    public DisposableTaskWrapper(ValueTask<IDisposable> disposableTask)
    {
        _task = Task.Run(async () =>
        {
            var disposable = await disposableTask.ConfigureAwait(false);
            if (_isDisposed)
            {
                disposable.Dispose();
            }
            else
            {
                _disposable = disposable;
            }
        });
    }

    private bool _isDisposed;
    public void Dispose()
    {
        if (_isDisposed) return;

        _disposable?.Dispose();
        if (_task.IsCompleted) _task.Dispose();

        _isDisposed = true;
    }
}
