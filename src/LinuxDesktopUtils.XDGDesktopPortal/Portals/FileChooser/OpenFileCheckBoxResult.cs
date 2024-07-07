using JetBrains.Annotations;

namespace LinuxDesktopUtils.XDGDesktopPortal;

public partial class FileChooserPortal
{
    /// <summary>
    /// Result of a checkbox.
    /// </summary>
    [PublicAPI]
    public sealed record OpenFileCheckBoxResult
    {
        /// <summary>
        /// Gets the ID of the checkbox.
        /// </summary>
        public required string Id { get; init; }

        /// <summary>
        /// Gets the value of the checkbox.
        /// </summary>
        public required bool Value { get; init; }
    }
}
