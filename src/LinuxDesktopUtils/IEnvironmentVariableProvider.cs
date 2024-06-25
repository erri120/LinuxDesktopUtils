using System;
using JetBrains.Annotations;

namespace LinuxDesktopUtils;

/// <summary>
/// Represents a provider for environment variables.
/// </summary>
[PublicAPI]
public interface IEnvironmentVariableProvider
{
    /// <summary>
    /// Gets an environment variable.
    /// </summary>
    /// <param name="name">Name of the environment variable.</param>
    /// <returns>The environment variable or <see langword="null"/>.</returns>
    string? Get(string name);
}

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
        if (string.IsNullOrWhiteSpace(value)) return null;
        return value;
    }
}
