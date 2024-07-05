using JetBrains.Annotations;
using TransparentValueObjects;

namespace LinuxDesktopUtils.XDGDesktopPortal;

/// <summary>
/// Represents an absolute path to a file.
/// </summary>
[PublicAPI]
[ValueObject<string>]
public readonly partial struct FilePath { }
