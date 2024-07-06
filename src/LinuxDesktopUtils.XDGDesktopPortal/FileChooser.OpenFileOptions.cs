using System.Collections.Generic;
using JetBrains.Annotations;
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

        // TODO: filters
        // TODO: current filter
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

            if (!string.IsNullOrEmpty(AcceptLabel))
                varDict.Add("accept_label", AcceptLabel);

            return varDict;
        }
    }
}

