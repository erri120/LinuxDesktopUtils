using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using OneOf;
using Tmds.DBus.Protocol;

namespace LinuxDesktopUtils.XDGDesktopPortal;

public partial class FileChooserPortal
{
    /// <summary>
    /// Results of <see cref="FileChooserPortal.OpenFileAsync"/>.
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
        public Uri[] SelectedFiles { get; internal set; } = [];

        /// <summary>
        /// Gets the filter that was selected.
        /// </summary>
        public OpenFileFilter? SelectedFilter { get; internal set; }

        /// <summary>
        /// Gets all choices made by the users.
        /// </summary>
        public OneOf<OpenFileComboBoxResult, OpenFileCheckBoxResult>[]? Choices { get; internal set; }

        internal static OpenFileResults From(OpenFileOptions options, Dictionary<string, VariantValue> varDict)
        {
            var res = new OpenFileResults();

            res.SelectedFiles = ParseSelectedFiles(varDict);

            if (varDict.TryGetValue("current_filter", out var filterVariantValue))
            {
                res.SelectedFilter = OpenFileFilter.FromVariant(filterVariantValue);
            }

            var choiceList = options.Choices;
            if (choiceList is not null)
            {
                res.Choices = ParseChoices(choiceList, varDict);
            }

            return res;
        }

        private static OneOf<OpenFileComboBoxResult, OpenFileCheckBoxResult>[] ParseChoices(
            OpenFileChoicesList input,
            Dictionary<string, VariantValue> varDict)
        {
            if (!varDict.TryGetValue("choices", out var choicesValue)) return [];
            VariantParsingException.ExpectArray(choicesValue, expectedItemType: VariantValueType.Struct);

            var list = new List<OneOf<OpenFileComboBoxResult, OpenFileCheckBoxResult>>(capacity: choicesValue.Count);
            for (var i = 0; i < choicesValue.Count; i++)
            {
                var element = choicesValue.GetItem(i);
                VariantParsingException.ExpectStruct(element, expectedCount: 2);

                var id = element.GetItem(0).GetString();
                var value = element.GetItem(1).GetString();

                if (!input.TryGet(id, out var found))
                    throw new KeyNotFoundException($"Results contain an unknown choice with id `{id}`");

                list.Add(found.Match<OneOf<OpenFileComboBoxResult, OpenFileCheckBoxResult>>(
                    f0: _ => new OpenFileComboBoxResult
                    {
                        Id = id,
                        Value = value,
                    },
                    f1: _ => new OpenFileCheckBoxResult
                    {
                        Id = id,
                        Value = string.Equals(value, "true", StringComparison.Ordinal) ||
                                (string.Equals(value, "false", StringComparison.Ordinal)
                                    ? false
                                    : throw new VariantParsingException($"Expected `false` or `true` but found `{value}`")),
                    }
                ));
            }

            foreach (var item in input)
            {
                var id = item.Match(
                    f0: x => x.Id,
                    f1: x => x.Id
                );

                var matches = list.Select(x => x.Match(
                    f0: comboBoxResult => comboBoxResult.Id,
                    f1: checkBoxResult => checkBoxResult.Id
                )).Any(resultId => resultId.Equals(id, StringComparison.Ordinal));

                if (matches) continue;

                list.Add(item.Match<OneOf<OpenFileComboBoxResult, OpenFileCheckBoxResult>>(
                    f0: comboBox => new OpenFileComboBoxResult
                    {
                        Id = id,
                        Value = comboBox.Choices.FirstOrDefault(x => x.IsDefault)?.Id ?? comboBox.Choices[0].Id,
                    },
                    f1: checkBox => new OpenFileCheckBoxResult
                    {
                        Id = id,
                        Value = checkBox.DefaultValue,
                    }
                ));
            }

            return list.ToArray();
        }

        private static Uri[] ParseSelectedFiles(Dictionary<string, VariantValue> varDict)
        {
            if (!varDict.TryGetValue("uris", out var urisValue))
                throw new KeyNotFoundException("Results don't contain selected files");

            VariantParsingException.ExpectArray(urisValue, expectedItemType: VariantValueType.String);
            var stringArray = urisValue.GetArray<string>();
            var selectedFiles = new Uri[stringArray.Length];

            for (var i = 0; i < stringArray.Length; i++)
            {
                var stringItem = stringArray[i];

                var selectedFileUri = new Uri(stringItem, UriKind.Absolute);
                if (!selectedFileUri.IsFile) throw new VariantParsingException($"URI is not a file, schema is `{selectedFileUri.Scheme}`");

                selectedFiles[i] = selectedFileUri;
            }

            return selectedFiles;
        }
    }
}

