using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using LinuxDesktopUtils.XDGDesktopPortal;
using Tmds.DBus.Protocol;

namespace LinuxDesktopUtils.Example;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var path = Path.Combine(Path.GetDirectoryName(Path.Combine(Environment.ProcessPath ?? "")) ?? "", "tmp.txt");
        await File.WriteAllTextAsync(path, "hello world").ConfigureAwait(false);

        using var cts = new CancellationTokenSource();

        using (PosixSignalRegistration.Create(PosixSignal.SIGINT, _ => cts.Cancel()))
        {
            try
            {
                var response = await OpenUriPortal.OpenFileInDirectoryAsync(
                    Connection.Session,
                    new WindowIdentifier.Empty(),
                    FilePath.From(path),
                    cancellationToken: cts.Token
                ).ConfigureAwait(false);

                Console.WriteLine($"Response: {response.ToString()}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception: {e}");
            }
        }
    }
}
