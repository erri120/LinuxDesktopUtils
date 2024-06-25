using JetBrains.Annotations;

namespace LinuxDesktopUtils;

/// <summary>
/// Represents the XDG runtime directory.
/// </summary>
[PublicAPI]
public readonly struct XDGRuntimeDirectory
{
    // ReSharper disable once InconsistentNaming
    private const string XDG_RUNTIME_DIR = nameof(XDG_RUNTIME_DIR);

    /// <summary>
    /// Resolves the path using the provider.
    /// </summary>
    public static string? ResolvePath(IEnvironmentVariableProvider provider)
    {
        var value = provider.Get(XDG_RUNTIME_DIR);
        return string.IsNullOrEmpty(value) ? null : value;
    }
}
