using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using OneOf;
using Tmds.DBus.Protocol;

namespace LinuxDesktopUtils.XDGDesktopPortal;

public partial class FileChooser
{
    /// <summary>
    /// Options for <see cref="FileChooser.OpenFileAsync"/>.
    /// </summary>
    [PublicAPI]
    public record OpenFileOptions : IPortalOptions
    {
        internal readonly string HandleToken = DBusHelper.CreateHandleToken();

        /// <summary>
        /// Label for the accept button. Mnemonic underlines are allowed.
        /// </summary>
        public string? AcceptLabel { get; init; }

        /// <summary>
        /// Whether the dialog should be modal.
        /// </summary>
        /// <remarks>
        /// Default is true.
        /// </remarks>
        public bool IsDialogModal { get; init; } = true;

        /// <summary>
        /// Whether multiple files can be selected or not.
        /// </summary>
        /// <remarks>
        /// Default is false, single-selection.
        /// </remarks>
        public bool AllowMultiple { get; init; }

        /// <summary>
        /// Whether to select for folders instead of files.
        /// </summary>
        /// <remarks>
        /// Default is false, to select files.
        /// </remarks>
        public bool SelectDirectories { get; init; }

        // public Array<Struct<string, Array<Struct<uint, string>>>>? Filters { get; init; }

        /// <summary>
        /// List of file filters for the user.
        /// </summary>
        /// <remarks>
        /// Note that filters are purely there to aid the user in making a useful selection.
        /// The portal may still allow the user to select files that don’t match any filter criteria,
        /// and applications must be prepared to handle that.
        /// </remarks>
        public OpenFileFilterList? Filters { get; init; }

        /// <summary>
        /// List of choices for the user.
        /// </summary>
        /// <remarks>
        /// These choices will be exposed in the file chooser UI to the user.
        /// </remarks>
        public OpenFileChoicesList? Choices { get; init; }

        /// <summary>
        /// Suggested folder from which teh file should be opened.
        /// </summary>
        /// <remarks>
        /// Portal implementations are free to ignore this option.
        /// </remarks>
        public Optional<DirectoryPath> SuggestedFolder { get; init; }

        /// <inheritdoc/>
        public Dictionary<string, Variant> ToVarDict()
        {
            var varDict = new Dictionary<string, Variant>(StringComparer.OrdinalIgnoreCase)
            {
                { "handle_token", HandleToken },
                { "modal", IsDialogModal },
                { "multiple", AllowMultiple },
                { "directory", SelectDirectories },
            };

            if (!string.IsNullOrEmpty(AcceptLabel)) varDict.Add("accept_label", AcceptLabel);
            if (Filters is not null)
            {
                var defaultFilterIndex = Filters.FindIndex(filter => filter.IsDefault);
                if (defaultFilterIndex != -1)
                {
                    var defaultFilter = Filters[defaultFilterIndex];
                    varDict.Add("current_filter", defaultFilter.ToVariant());
                }

                if (defaultFilterIndex == -1 || Filters.Count > 1)
                    varDict.Add("filters", Filters.ToVariant());
            }

            if (Choices is not null) varDict.Add("choices", Choices.ToVariant());
            if (SuggestedFolder.HasValue)
            {
                // The byte array contains a path in the same encoding as the file system, and it’s expected to be terminated by a nul byte.
                // NOTE(erri120): no idea why they made this so complicated, we're just going to default to UTF8
                var bytes = SuggestedFolder.Value.ToByteArray(encoding: Encoding.UTF8, nullTerminated: true);
                varDict.Add("current_folder", Variant.FromArray(new Array<byte>(bytes)));
            }

            return varDict;
        }
    }

    /// <summary>
    /// Represents a list of <see cref="OpenFileFilter"/>.
    /// </summary>
    [PublicAPI]
    public class OpenFileFilterList : List<OpenFileFilter>
    {
        internal Array<Struct<string, Array<Struct<uint, string>>>> ToVariant()
        {
            var enumerable = this.Select(filter => filter.ToVariant());
            return new Array<Struct<string, Array<Struct<uint, string>>>>(enumerable);
        }
    }

    /// <summary>
    /// Represents a filter for the user.
    /// </summary>
    [PublicAPI]
    public sealed record OpenFileFilter
    {
        /// <summary>
        /// Gets or initializes the user-visible name of the filter.
        /// </summary>
        public required string FilterName { get; init; }

        /// <summary>
        /// Gets or initializes an array of patterns.
        /// </summary>
        /// <remarks>
        /// Patterns are case-sensitive. <c>*.ico</c> won't match <c>icon.ICO</c>.
        /// </remarks>
        public required OneOf<GlobPattern, MimeType>[] Patterns { get; init; }

        /// <summary>
        /// Whether this filter is the default for the dialog.
        /// </summary>
        public bool IsDefault { get; init; }

        internal Struct<string, Array<Struct<uint, string>>> ToVariant()
        {
            var enumerable = Patterns.Select((value, i) =>
            {
                var s = value.Match(
                    f0: x => x.Value,
                    f1: x => x.Value
                );

                return new Struct<uint, string>((uint)i, s);
            });

            var arr = new Array<Struct<uint, string>>(enumerable);
            return new Struct<string, Array<Struct<uint, string>>>(FilterName, arr);
        }

        internal static OpenFileFilter FromVariant(VariantValue variantValue)
        {
            VariantParsingException.ExpectType(variantValue, VariantValueType.Struct);
            VariantParsingException.ExpectCount(variantValue, expectedCount: 2);

            var filterName = variantValue.GetItem(0).GetString();
            return new OpenFileFilter
            {
                FilterName = filterName,
                Patterns = [],
            };
        }

        /// <inheritdoc/>
        public bool Equals(OpenFileFilter? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(FilterName, other.FilterName, StringComparison.OrdinalIgnoreCase);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return FilterName.GetHashCode(StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// Represents a list of <see cref="OpenFileComboBox"/> or <see cref="OpenFileCheckBox"/>.
    /// </summary>
    [PublicAPI]
    public class OpenFileChoicesList : List<OneOf<OpenFileComboBox, OpenFileCheckBox>>
    {
        internal bool TryGet(string id, out OneOf<OpenFileComboBox, OpenFileCheckBox> found)
        {
            foreach (var item in this)
            {
                var matches = item.Match(
                    f0: x => string.Equals(x.Id, id, StringComparison.Ordinal),
                    f1: x => string.Equals(x.Id, id, StringComparison.Ordinal)
                );

                if (!matches) continue;
                found = item;
                return true;
            }

            found = default;
            return false;
        }

        internal Array<Struct<string, string, Array<Struct<string, string>>, string>> ToVariant()
        {
            var enumerable = this.Select(value => value.Match(
                f0: x => x.ToVariant(),
                f1: x => x.ToVariant()
            ));

            return new Array<Struct<string, string, Array<Struct<string, string>>, string>>(enumerable);
        }
    }

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

