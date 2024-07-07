using JetBrains.Annotations;

namespace LinuxDesktopUtils.XDGDesktopPortal;

public partial class SecretPortal
{
    /// <summary>
    /// Results of <see cref="SecretPortal.RetrieveSecretAsync"/>.
    /// </summary>
    [PublicAPI]
    public record RetrieveSecretResult
    {
        /// <summary>
        /// Gets the secret.
        /// </summary>
        public required byte[] Secret { get; init; }
    }
}

