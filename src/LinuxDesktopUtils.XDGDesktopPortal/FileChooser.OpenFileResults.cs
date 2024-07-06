using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Tmds.DBus.Protocol;

namespace LinuxDesktopUtils.XDGDesktopPortal;

public partial class FileChooser
{
    /// <summary>
    /// Results of <see cref="FileChooser.OpenFileAsync"/>.
    /// </summary>
    [PublicAPI]
    public record OpenFileResults
    {
        /// <summary>
        /// Gets an array of the selected files.
        /// </summary>
        /// <remarks>
        /// All URIs have the <c>file</c> scheme.
        /// </remarks>
        public Uri[] SelectedFiles { get; internal set; } = Array.Empty<Uri>();

        // TODO: choices
        // TODO: current filter

        internal static OpenFileResults From(Dictionary<string, VariantValue> varDict)
        {
            var res = new OpenFileResults();

            res.SelectedFiles = ParseSelectedFiles(varDict);

            return res;
        }

        private static Uri[] ParseSelectedFiles(Dictionary<string, VariantValue> varDict)
        {
            if (!varDict.TryGetValue("uris", out var urisValue)) throw new NotImplementedException();

            if (urisValue.Type != VariantValueType.Array) throw new NotImplementedException();
            if (urisValue.ItemType != VariantValueType.String) throw new NotImplementedException();
            var stringArray = urisValue.GetArray<string>();
            var selectedFiles = new Uri[stringArray.Length];

            for (var i = 0; i < stringArray.Length; i++)
            {
                var stringItem = stringArray[i];

                if (!Uri.TryCreate(stringItem, UriKind.Absolute, out var selectedFileUri)) throw new NotImplementedException();
                if (!selectedFileUri.IsFile) throw new NotImplementedException();

                selectedFiles[i] = selectedFileUri;
            }

            return selectedFiles;
        }
    }
}

