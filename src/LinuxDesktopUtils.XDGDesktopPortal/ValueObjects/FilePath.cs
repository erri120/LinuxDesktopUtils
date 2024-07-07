using System.Text;
using JetBrains.Annotations;
using TransparentValueObjects;

namespace LinuxDesktopUtils.XDGDesktopPortal;

/// <summary>
/// Represents an absolute path to a file.
/// </summary>
[PublicAPI]
[ValueObject<string>]
public readonly partial struct FilePath
{
    internal byte[] ToByteArray(Encoding encoding, bool nullTerminated)
    {
        var byteCount = encoding.GetByteCount(Value);
        var arraySize = nullTerminated ? byteCount + 1 : byteCount;
        var bytes = new byte[arraySize];
        _ = encoding.GetBytes(Value, bytes);
        return bytes;
    }
}
