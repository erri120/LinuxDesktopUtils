using JetBrains.Annotations;
using TransparentValueObjects;

namespace LinuxDesktopUtils.XDGDesktopPortal;

/// <summary>
/// Represents a glob pattern like <c>*.png</c>.
/// </summary>
[PublicAPI]
[ValueObject<string>]
public readonly partial struct GlobPattern { }
