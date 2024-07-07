using JetBrains.Annotations;
using Tmds.DBus.Protocol;

namespace LinuxDesktopUtils.XDGDesktopPortal;

public partial class FileChooserPortal
{
    /// <summary>
    /// Represents a checkbox added to the file chooser.
    /// </summary>
    [PublicAPI]
    public sealed record OpenFileCheckBox
    {
        /// <summary>
        /// Gets or initializes the unique ID of the checkbox.
        /// </summary>
        public required string Id { get; init; }

        /// <summary>
        /// Gets or initializes the user-visible label.
        /// </summary>
        public required string Label { get; init; }

        /// <summary>
        /// Gets or initializes default value of the checkbox.
        /// </summary>
        public required bool DefaultValue { get; init; }

        internal Struct<string, string, Array<Struct<string, string>>, string> ToVariant()
        {
            // As a special case, passing an empty array for the list of choices indicates a boolean choice
            // that is typically displayed as a check button, using “true” and “false” as the choices.
            return new Struct<string, string, Array<Struct<string, string>>, string>(
                Id,
                Label,
                [],
                DefaultValue ? "true" : "false"
            );
        }
    }
}
