using JetBrains.Annotations;

namespace LinuxDesktopUtils.XDGDesktopPortal;

/// <summary>
/// Represents a response without results.
/// </summary>
[PublicAPI]
public record Response
{
    /// <summary>
    /// Gets the status of the response.
    /// </summary>
    public required ResponseStatus Status { get; init; }
}

/// <summary>
/// Represents a response with results.
/// </summary>
[PublicAPI]
public record Response<T> : Response
    where T : notnull
{
    /// <summary>
    /// Gets the results of the response.
    /// </summary>
    public required Optional<T> Results { get; init; }
}
