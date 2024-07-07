using JetBrains.Annotations;

namespace LinuxDesktopUtils.XDGDesktopPortal;

public partial class FileChooserPortal
{
    /// <summary>
    /// Result of a combo box.
    /// </summary>
    [PublicAPI]
    public sealed record OpenFileComboBoxResult
    {
        /// <summary>
        /// Gets the ID of the combo box.
        /// </summary>
        public required string Id { get; init; }

        /// <summary>
        /// Gets the selected value.
        /// </summary>
        public required string Value { get; init; }
    }
}
