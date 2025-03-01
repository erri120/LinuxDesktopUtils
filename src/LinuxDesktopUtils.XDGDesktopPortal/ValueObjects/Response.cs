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
    /// <remarks>
    /// This is only available if <see cref="Response{T}.Status"/> is <see cref="ResponseStatus.Success"/>.
    /// </remarks>
    public required Optional<T> Results { get; init; }
}
