using System;
using JetBrains.Annotations;

namespace LinuxDesktopUtils.XDGBaseDirectories;

/// <summary>
/// Implementation of <see cref="IEnvironmentVariableProvider"/> using <see cref="Environment.GetEnvironmentVariable(string)"/>.
/// </summary>
[PublicAPI]
public class EnvironmentVariableProvider : IEnvironmentVariableProvider
{
    /// <summary>
    /// Instance of <see cref="EnvironmentVariableProvider"/>.
    /// </summary>
    public static readonly IEnvironmentVariableProvider Instance = new EnvironmentVariableProvider();

    /// <inheritdoc/>
    public string? Get(string name)
    {
        var value = Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }
}
