using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using LinuxDesktopUtils.XDGDesktopPortal;

namespace LinuxDesktopUtils.Example;

[SuppressMessage("Usage", "MA0004:Use Task.ConfigureAwait")]
public static class Program
{
    public static async Task Main(string[] args)
    {
        await RunAsync();
        Console.WriteLine("done");
    }

    private static async Task RunAsync()
    {
        var path = Path.Combine(Path.GetDirectoryName(Path.Combine(Environment.ProcessPath ?? "")) ?? "", "tmp.txt");
        await File.WriteAllTextAsync(path, "hello world");

        await using var connectionManager = await DesktopPortalConnectionManager.ConnectAsync();

        using var cts = new CancellationTokenSource();
        using var _ = PosixSignalRegistration.Create(PosixSignal.SIGINT, _ => cts.Cancel());

        try
        {
            var openUriPortal = await connectionManager.GetOpenUriPortalAsync().ConfigureAwait(false);
            var response = await openUriPortal.OpenFileAsync(
                file: FilePath.From(path),
                cancellationToken: cts.Token
            ).ConfigureAwait(false);

            Console.WriteLine(response);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}
