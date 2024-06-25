using System;
using JetBrains.Annotations;

namespace LinuxDesktopUtils;

[PublicAPI]
public readonly struct XDGOrderedBaseDirectories
{
    private readonly string _environmentVariableName;
    private readonly string[] _defaultPaths;

    internal XDGOrderedBaseDirectories(string environmentVariableName, string[] defaultPaths)
    {
        _environmentVariableName = environmentVariableName;
        _defaultPaths = defaultPaths;
    }

    /// <summary>
    /// Resolves the paths using the provider.
    /// </summary>
    public string[] ResolvesPaths(IEnvironmentVariableProvider provider)
    {
        var value = provider.Get(_environmentVariableName);
        if (string.IsNullOrEmpty(value)) return _defaultPaths;
        return value.Split(':', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
    }
}
