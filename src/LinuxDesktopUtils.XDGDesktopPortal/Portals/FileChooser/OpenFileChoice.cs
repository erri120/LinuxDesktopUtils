using JetBrains.Annotations;

namespace LinuxDesktopUtils.XDGDesktopPortal;

public partial class FileChooserPortal
{
    /// <summary>
    /// Represents a choice of a combo box in the file chooser.
    /// </summary>
    [PublicAPI]
    public sealed record OpenFileChoice
    {
        /// <summary>
        /// Gets or initializes the unique ID of the choice.
        /// </summary>
        public required string Id { get; init; }

        /// <summary>
        /// Gets or initializes the user-visible label.
        /// </summary>
        public required string Label { get; init; }

        /// <summary>
        /// Whether the choice is selected by default.
        /// </summary>
        public bool IsDefault { get; init; }
    }
}
