using JetBrains.Annotations;

namespace LinuxDesktopUtils.XDGDesktopPortal;

/// <summary>
/// Represents a response from a request.
/// </summary>
[PublicAPI]
public enum ResponseStatus : uint
{
    /// <summary>
    /// Success, the request is carried out.
    /// </summary>
    Success = 0,

    /// <summary>
    /// The user cancelled the interaction.
    /// </summary>
    UserCancelled = 1,

    /// <summary>
    /// The user interaction was ended in some other way.
    /// </summary>
    Aborted = 2,
}
