using JetBrains.Annotations;

namespace LinuxDesktopUtils.XDGBaseDirectories;

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
