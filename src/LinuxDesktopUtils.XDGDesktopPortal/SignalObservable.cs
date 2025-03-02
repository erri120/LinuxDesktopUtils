using System;
using System.Threading.Tasks;
using R3;

namespace LinuxDesktopUtils.XDGDesktopPortal;

internal class SignalObservable<T> : Observable<T>
{
    public delegate ValueTask<IDisposable> WatchAsync(Observer<T> observer);

    private readonly WatchAsync _watchAsync;
    public SignalObservable(WatchAsync watchAsync)
    {
        _watchAsync = watchAsync;
    }

    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        var disposableTask = _watchAsync(observer);
        return new DisposableTaskWrapper(disposableTask);
    }
}
