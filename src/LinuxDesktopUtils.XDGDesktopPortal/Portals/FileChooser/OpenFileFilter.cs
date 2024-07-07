using System;
using System.Linq;
using JetBrains.Annotations;
using OneOf;
using Tmds.DBus.Protocol;

namespace LinuxDesktopUtils.XDGDesktopPortal;

public partial class FileChooserPortal
{
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
}
