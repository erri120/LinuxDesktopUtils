using System.Collections.Generic;
using System.Linq;
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
        /// Set a default filter.
        /// </summary>
        /// <remarks>
        /// If <see cref="Filters"/> is non-null, the default filter must be in that list.
        /// If the list is null, the default filter will be applied unconditionally.
        /// </remarks>
        public OpenFileFilter? DefaultFilter { get; init; }

        // TODO: choices
        // TODO: current folder

        /// <inheritdoc/>
        public Dictionary<string, Variant> ToVarDict()
        {
            var varDict = new Dictionary<string, Variant>(System.StringComparer.OrdinalIgnoreCase)
            {
                { "handle_token", HandleToken },
                { "modal", IsDialogModal },
                { "multiple", AllowMultiple },
                { "directory", SelectDirectories },
            };

            if (!string.IsNullOrEmpty(AcceptLabel)) varDict.Add("accept_label", AcceptLabel);
            if (Filters is not null) varDict.Add("filters", Filters.ToVariant());
            if (DefaultFilter is not null) varDict.Add("current_filter", DefaultFilter.ToVariant());

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
    public class OpenFileFilter
    {
        /// <summary>
        /// Gets the user-visible name of the filter.
        /// </summary>
        public required string FilterName { get; init; }

        /// <summary>
        /// Gets array of patterns.
        /// </summary>
        /// <remarks>
        /// Patterns are case-sensitive. <c>*.ico</c> won't match <c>icon.ICO</c>.
        /// </remarks>
        public required OneOf<GlobPattern, MimeType>[] Patterns { get; init; }

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
    }
}

