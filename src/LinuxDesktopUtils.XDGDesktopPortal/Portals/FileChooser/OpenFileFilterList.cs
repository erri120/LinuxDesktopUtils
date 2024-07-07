using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Tmds.DBus.Protocol;

namespace LinuxDesktopUtils.XDGDesktopPortal;

public partial class FileChooserPortal
{
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
}
