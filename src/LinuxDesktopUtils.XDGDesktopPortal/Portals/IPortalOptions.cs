using System.Collections.Generic;
using JetBrains.Annotations;
using Tmds.DBus.Protocol;

namespace LinuxDesktopUtils.XDGDesktopPortal;

/// <summary>
/// Can be converted into an options VarDict.
/// </summary>
[PublicAPI]
public interface IPortalOptions
{
    /// <summary>
    /// Creates a dictionary out of the values.
    /// </summary>
#pragma warning disable MA0016
    Dictionary<string, VariantValue> ToVarDict();
#pragma warning restore MA0016
}
