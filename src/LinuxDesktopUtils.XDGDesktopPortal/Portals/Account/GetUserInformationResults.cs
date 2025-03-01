using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Tmds.DBus.Protocol;

namespace LinuxDesktopUtils.XDGDesktopPortal;

public partial class AccountPortal
{
    /// <summary>
    /// Results of <see cref="AccountPortal.GetUserInformationAsync"/>.
    /// </summary>
    [PublicAPI]
    public record GetUserInformationResults
    {
        /// <summary>
        /// The user id.
        /// </summary>
        public string? UserId { get; set; }

        /// <summary>
        /// The user's real name.
        /// </summary>
        public string? UserName { get; set; }

        /// <summary>
        /// The URI of an image file for the user's avatar photo.
        /// </summary>
        public Uri? UserImage { get; set; }

        internal static GetUserInformationResults From(Dictionary<string, VariantValue> varDict)
        {
            var res = new GetUserInformationResults();

            if (varDict.TryGetValue("id", out var idValue))
            {
                var id = idValue.GetString();
                if (!string.IsNullOrEmpty(id)) res.UserId = id;
            }

            if (varDict.TryGetValue("name", out var nameValue))
            {
                var name = nameValue.GetString();
                if (!string.IsNullOrEmpty(name)) res.UserName = name;
            }

            if (varDict.TryGetValue("image", out var imageValue))
            {
                var image = imageValue.GetString();
                if (Uri.TryCreate(image, UriKind.Absolute, out var uri))
                {
                    if (!uri.IsFile) throw new NotSupportedException($"Portal returned a non-file URI `{uri}`");
                    res.UserImage = uri;
                }
            }

            return res;
        }
    }
}
