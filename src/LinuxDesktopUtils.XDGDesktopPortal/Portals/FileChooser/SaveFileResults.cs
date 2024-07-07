using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using OneOf;
using Tmds.DBus.Protocol;

namespace LinuxDesktopUtils.XDGDesktopPortal;

public partial class FileChooserPortal
{
    /// <summary>
    /// Results of <see cref="FileChooserPortal.SaveFileResults"/>.
    /// </summary>
    [PublicAPI]
    public record SaveFileResults
    {
        /// <summary>
        /// Gets the selected file location.
        /// </summary>
        /// <remarks>
        /// URI has the <c>file</c> scheme.
        /// </remarks>
        public Uri SelectedFileLocation { get; internal init; } = null!;

        /// <summary>
        /// Gets the filter that was selected.
        /// </summary>
        public OpenFileFilter? SelectedFilter { get; internal set; }

        /// <summary>
        /// Gets all choices made by the users.
        /// </summary>
        public OneOf<OpenFileComboBoxResult, OpenFileCheckBoxResult>[]? Choices { get; internal set; }

        internal static SaveFileResults From(SaveFileOptions options, Dictionary<string, VariantValue> varDict)
        {
            var res = new SaveFileResults
            {
                SelectedFileLocation = OpenFileResults.ParseSelectedFiles(varDict)[0],
            };

            if (varDict.TryGetValue("current_filter", out var filterVariantValue))
            {
                res.SelectedFilter = OpenFileFilter.FromVariant(filterVariantValue);
            }

            var choiceList = options.Choices;
            if (choiceList is not null)
            {
                res.Choices = OpenFileResults.ParseChoices(choiceList, varDict);
            }

            return res;
        }
    }
}

