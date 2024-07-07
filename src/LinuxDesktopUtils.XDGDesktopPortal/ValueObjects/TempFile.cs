using System;
using System.IO;
using TransparentValueObjects;

namespace LinuxDesktopUtils.XDGDesktopPortal;

[ValueObject<string>]
internal readonly partial struct TempFile : IDisposable
{
    public static TempFile New() => TempFile.From(Path.GetTempFileName());

    public void Dispose()
    {
        try
        {
            File.Delete(Value);
        }
        catch (Exception)
        {
            // ignored
        }
    }
}
