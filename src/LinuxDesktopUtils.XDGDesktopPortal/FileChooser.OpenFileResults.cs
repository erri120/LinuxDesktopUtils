using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using OneOf;
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

            if (choicesValue.Type != VariantValueType.Array) throw new NotImplementedException();
            if (choicesValue.ItemType != VariantValueType.Struct) throw new NotImplementedException();

            var list = new List<OneOf<OpenFileComboBoxResult, OpenFileCheckBoxResult>>(capacity: choicesValue.Count);
            for (var i = 0; i < choicesValue.Count; i++)
            {
                var element = choicesValue.GetItem(i);
                if (element.Type != VariantValueType.Struct) throw new NotImplementedException();
                if (element.Count != 2) throw new NotImplementedException();

                var idValue = element.GetItem(0);
                if (idValue.Type != VariantValueType.String) throw new NotImplementedException();

                var valueValue = element.GetItem(1);
                if (valueValue.Type != VariantValueType.String) throw new NotImplementedException();

                var id = idValue.GetString();
                var value = valueValue.GetString();

                if (!input.TryGet(id, out var found)) throw new NotImplementedException();
                list.Add(found.Match<OneOf<OpenFileComboBoxResult, OpenFileCheckBoxResult>>(
                    f0: _ => new OpenFileComboBoxResult
                    {
                        Id = id,
                        Value = value,
                    },
                    f1: _ => new OpenFileCheckBoxResult
                    {
                        Id = id,
                        Value = string.Equals(value, "true", StringComparison.Ordinal) || (string.Equals(value, "false", StringComparison.Ordinal) ? false : throw new NotImplementedException()),
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
                    f0: x => x.Id,
                    f1: x => x.Id
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

    /// <summary>
    /// Result of a checkbox.
    /// </summary>
    [PublicAPI]
    public sealed record OpenFileCheckBoxResult
    {
        /// <summary>
        /// Gets the ID of the checkbox.
        /// </summary>
        public required string Id { get; init; }

        /// <summary>
        /// Gets the value of the checkbox.
        /// </summary>
        public required bool Value { get; init; }
    }
}

