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
}
