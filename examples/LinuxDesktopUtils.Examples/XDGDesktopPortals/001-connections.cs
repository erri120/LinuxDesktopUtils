using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using LinuxDesktopUtils.XDGDesktopPortal;

namespace LinuxDesktopUtils.Examples;

// This first today will go over the DesktopPortalConnectionManager and how to connect to
// the D-Bus.

[SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "example")]
[SuppressMessage("ReSharper", "UnusedType.Local", Justification = "example")]
file class MyClass : IAsyncDisposable
{
    // There should only be one instance of the connection manager.
    private readonly DesktopPortalConnectionManager _connectionManager;

    // If you use DI, you should register it as a singleton.
    private MyClass(DesktopPortalConnectionManager connectionManager)
    {
        _connectionManager = connectionManager;
    }

    public static async Task<MyClass> CreateAsync()
    {
        // The connection manager is created async, so you might need to use an async factory.
        var connectionManager = await DesktopPortalConnectionManager.ConnectAsync(
            // If your application only has one Window, you can pass the window identifier to
            // the connection manager. Every request would use the provided window identifier.
            // Alternatively, you can pass an identifier in each request or leave it as None.
            defaultWindowIdentifier: Optional<WindowIdentifier>.None,

            // You can manually pass the address of the D-Bus, by default the library will use
            // the session connection. Unless you have very good reasons to do so, this doesn't
            // need to be changed.
            address: Optional<string>.None
        );

        return new MyClass(connectionManager);
    }

    public async ValueTask DisposeAsync()
    {
        // Don't forget to dispose of the connection manager.
        await _connectionManager.DisposeAsync();
    }
}
