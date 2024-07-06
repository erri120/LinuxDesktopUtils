using System;
using System.IO;
using JetBrains.Annotations;

namespace LinuxDesktopUtils;

/// <summary>
/// Represents a XDG Base Directory for user-specific files.
/// </summary>
[PublicAPI]
public readonly struct XDGHomeDirectory
{
    private readonly string _environmentVariableName;
    private readonly string _defaultPath;

    internal XDGHomeDirectory(string environmentVariableName, string defaultPath)
    {
        _environmentVariableName = environmentVariableName;
        _defaultPath = defaultPath;
    }

    /// <summary>
    /// Resolves the path using the provider.
    /// </summary>
    /// <exception cref="PlatformNotSupportedException">Thrown when `$HOME` isn't available as an environment variable.</exception>
    public string ResolvePath(IEnvironmentVariableProvider provider)
    {
        var environmentVariableValue = provider.Get(_environmentVariableName);
        if (!string.IsNullOrEmpty(environmentVariableValue)) return environmentVariableValue;

        var home = provider.Get("HOME");
        if (string.IsNullOrEmpty(home))
            throw new PlatformNotSupportedException($"Environment variable `$HOME` was not found. This value is required by the specification for the fallback value of `${_environmentVariableName}`");

        return Path.Combine(home, _defaultPath);
    }
}
