using JetBrains.Annotations;

namespace LinuxDesktopUtils;

/// <summary>
/// XDG Base Directories according to Version 0.8 of the Specification
/// </summary>
/// <remarks>
/// https://specifications.freedesktop.org/basedir-spec/basedir-spec-0.8.html
/// </remarks>
[PublicAPI]
public static class XDGBaseDirectories
{
    // ReSharper disable InconsistentNaming
    private const string XDG_DATA_HOME = nameof(XDG_DATA_HOME);
    private const string XDG_CONFIG_HOME = nameof(XDG_CONFIG_HOME);
    private const string XDG_STATE_HOME = nameof(XDG_STATE_HOME);
    private const string XDG_DATA_DIRS = nameof(XDG_DATA_DIRS);
    private const string XDG_CONFIG_DIRS = nameof(XDG_CONFIG_DIRS);
    private const string XDG_CACHE_HOME = nameof(XDG_CACHE_HOME);
    private const string XDG_RUNTIME_DIR = nameof(XDG_RUNTIME_DIR);
    // ReSharper restore InconsistentNaming

    /// <summary>
    /// URL to the specification.
    /// </summary>
    public const string SpecificationUrl = "https://specifications.freedesktop.org/basedir-spec/basedir-spec-0.8.html";

    /// <summary>
    /// Version of the specification.
    /// </summary>
    public const string SpecificationVersion = "0.8";

    /// <summary>
    /// Directory for user-specific data files.
    /// </summary>
    /// <remarks>
    /// <c>$XDG_DATA_HOME</c> defines the base directory relative to which user-specific data files should be stored.
    /// If <c>$XDG_DATA_HOME</c> is either not set or empty, a default equal to <c>$HOME/.local/share</c> will be used.
    /// </remarks>
    public static readonly XDGHomeDirectory DataHome = new(XDG_DATA_HOME, ".local/share");

    /// <summary>
    /// Directory for user-specific configuration files.
    /// </summary>
    /// <remarks>
    /// <c>$XDG_CONFIG_HOME</c> defines the base directory relative to which user-specific configuration files should be stored.
    /// If <c>$XDG_CONFIG_HOME</c> is either not set or empty, a default equal to <c>$HOME/.config</c> will be used.
    /// </remarks>
    public static readonly XDGHomeDirectory ConfigHome = new(XDG_CONFIG_HOME, ".config");

    /// <summary>
    /// Directory for user-specific state files.
    /// </summary>
    /// <remarks>
    /// <c>$XDG_STATE_HOME</c> defines the base directory relative to which user-specific state files should be stored.
    /// If <c>$XDG_STATE_HOME</c> is either not set or empty, a default equal to <c>$HOME/.local/state</c> will be used.
    /// </remarks>
    public static readonly XDGHomeDirectory StateHome = new(XDG_STATE_HOME, ".local/state");

    // TODO: user-specific executable files

    /// <summary>
    /// Preference-ordered set of base directories to search for data files in addition to <see cref="DataHome"/>.
    /// </summary>
    /// <remarks>
    /// If <c>$XDG_DATA_DIRS</c> is either not set or empty, a value equal to <c>/usr/local/share/:/usr/share/</c> will be used.
    /// </remarks>
    public static readonly XDGOrderedBaseDirectories DataDirs = new(XDG_DATA_DIRS, new []{"/usr/local/share", "/usr/share"});

    /// <summary>
    /// Preference-ordered set of base directories to search for configuration files in addition to <see cref="ConfigHome"/>.
    /// </summary>
    /// <remarks>
    /// If <c>$XDG_CONFIG_DIRS</c> is either not set or empty, a value equal to <c>/etc/xdg</c> will be used.
    /// </remarks>
    public static readonly XDGOrderedBaseDirectories ConfigDirs = new(XDG_CONFIG_DIRS, new []{"/etc/xdg"});

    /// <summary>
    /// Directory for user-specific non-essential data files.
    /// </summary>
    /// <remarks>
    /// <c>$XDG_CACHE_HOME</c> defines the base directory relative to which user-specific non-essential data files should be stored.
    /// If <c>$XDG_CACHE_HOME</c> is either not set or empty, a default equal to <c>$HOME/.cache</c> will be used.
    /// </remarks>
    public static readonly XDGHomeDirectory CacheHome = new(XDG_STATE_HOME, ".cache");

    // TODO: XDG_RUNTIME_DIR
}
