using JetBrains.Annotations;
using OneOf;
using TransparentValueObjects;

namespace LinuxDesktopUtils.XDGDesktopPortal;

/// <summary>
/// Represents a window identifier.
/// </summary>
[PublicAPI]
public partial struct WindowIdentifier
{
    private readonly OneOf<Empty, X11, Wayland> _value;

    /// <summary>
    /// Constructor.
    /// </summary>
    public WindowIdentifier(OneOf<Empty, X11, Wayland> value)
    {
        _value = value;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return _value.Match(
            f0: value => value.ToString(),
            f1: value => value.ToString(),
            f2: value => value.ToString()
        );
    }

    /// <summary>
    /// Implicit operator.
    /// </summary>
    public static implicit operator WindowIdentifier(X11 value) => new(value);

    /// <summary>
    /// Implicit operator.
    /// </summary>
    public static implicit operator WindowIdentifier(Wayland value) => new(value);

    /// <summary>
    /// Implicit operator.
    /// </summary>
    public static implicit operator WindowIdentifier(Empty value) => new(value);

    /// <summary>
    /// Represents an X11 window identifier.
    /// </summary>
    /// <remarks>
    /// The string must be in hexadecimal notation.
    /// </remarks>
    [PublicAPI]
    [ValueObject<string>]
    public readonly partial struct X11
    {
        /// <inheritdoc/>
        public override string ToString() => $"x11:{Value}";
    }

    /// <summary>
    /// Represents a Wayland window identifier.
    /// </summary>
    /// <remarks>
    /// The string must be a surface handle obtained with the xdg_foreign protocol.
    /// https://gitlab.freedesktop.org/wayland/wayland-protocols/-/blob/main/unstable/xdg-foreign/xdg-foreign-unstable-v2.xml
    /// </remarks>
    [PublicAPI]
    [ValueObject<string>]
    public readonly partial struct Wayland
    {
        /// <inheritdoc/>
        public override string ToString() => $"wayland:{Value}";
    }

    /// <summary>
    /// Represents an empty window identifier.
    /// </summary>
    public readonly struct Empty
    {
        /// <inheritdoc/>
        public override string ToString() => string.Empty;
    }
}
