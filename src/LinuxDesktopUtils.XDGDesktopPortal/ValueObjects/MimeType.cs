using JetBrains.Annotations;
using TransparentValueObjects;

namespace LinuxDesktopUtils.XDGDesktopPortal;

/// <summary>
/// Represents a MIME type like <c>image/png</c>.
/// </summary>
[PublicAPI]
[ValueObject<string>]
public readonly partial struct MimeType { }
