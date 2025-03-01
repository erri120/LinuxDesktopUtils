using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using Tmds.DBus.Protocol;

namespace LinuxDesktopUtils.XDGDesktopPortal;

public partial class FileChooserPortal
{
    /// <summary>
    /// Options for <see cref="FileChooserPortal.SaveFileAsync"/>.
    /// </summary>
    [PublicAPI]
    public record SaveFileOptions : IPortalOptions
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
        /// Suggested name of the file.
        /// </summary>
        public string? SuggestedFileName { get; init; }

        /// <summary>
        /// Suggested folder from which the file should be opened.
        /// </summary>
        /// <remarks>
        /// Portal implementations are free to ignore this option.
        /// </remarks>
        public Optional<DirectoryPath> SuggestedFolder { get; init; }

        /// <summary>
        /// The current file, when saving an existing file.
        /// </summary>
        public Optional<FilePath> CurrentFile { get; init; }

        /// <inheritdoc/>
        public Dictionary<string, VariantValue> ToVarDict()
        {
            var varDict = new Dictionary<string, VariantValue>(StringComparer.OrdinalIgnoreCase)
            {
                { "handle_token", HandleToken },
                { "modal", IsDialogModal },
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
            if (SuggestedFileName is not null) varDict.Add("current_name", SuggestedFileName);

            if (SuggestedFolder.HasValue)
            {
                // The byte array contains a path in the same encoding as the file system, and it’s expected to be terminated by a nul byte.
                var bytes = SuggestedFolder.Value.ToByteArray(encoding: Encoding.UTF8, nullTerminated: true);
                varDict.Add("current_folder", VariantValue.Array(bytes));
            }

            if (CurrentFile.HasValue)
            {
                // The byte array contains a path in the same encoding as the file system, and it’s expected to be terminated by a nul byte.
                var bytes = CurrentFile.Value.ToByteArray(encoding: Encoding.UTF8, nullTerminated: true);
                varDict.Add("current_file", VariantValue.Array(bytes));
            }

            return varDict;
        }
    }
}
