using System.Linq;
using JetBrains.Annotations;
using Tmds.DBus.Protocol;

namespace LinuxDesktopUtils.XDGDesktopPortal;

public partial class FileChooserPortal
{
    /// <summary>
    /// Represents a combo box added to the file chooser.
    /// </summary>
    [PublicAPI]
    public sealed record OpenFileComboBox
    {
        /// <summary>
        /// Gets or initializes the unique ID of the combo box.
        /// </summary>
        public required string Id { get; init; }

        /// <summary>
        /// Gets or initializes the user-visible label.
        /// </summary>
        public required string Label { get; init; }

        /// <summary>
        /// Gets or initializes the array of choices in the combo box.
        /// </summary>
        public required OpenFileChoice[] Choices { get; init; }

        internal Struct<string, string, Array<Struct<string, string>>, string> ToVariant()
        {
            var choiceEnumerable = Choices.Select(choice => new Struct<string, string>(choice.Id, choice.Label));
            var choiceArray = new Array<Struct<string, string>>(choiceEnumerable);

            var defaultChoiceId = Choices.FirstOrDefault(choice => choice.IsDefault)?.Id;
            return new Struct<string, string, Array<Struct<string, string>>, string>(Id, Label, choiceArray, defaultChoiceId ?? string.Empty);
        }
    }
}
