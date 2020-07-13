using System;
using System.Collections.Generic;

namespace Cloud.Core.Messaging.AzureStorageQueue.Models
{
    /// <summary>
    /// Class Signed Access Config.
    /// Implements the <see cref="ISignedAccessConfig" />
    /// </summary>
    /// <seealso cref="ISignedAccessConfig" />
    public class SignedAccessConfig : ISignedAccessConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SignedAccessConfig"/> class.
        /// </summary>
        /// <param name="permissions">The permissions.</param>
        /// <param name="accessExpiry">The access expiry.</param>
        public SignedAccessConfig(List<AccessPermission> permissions, DateTimeOffset? accessExpiry)
        {
            AccessPermissions = permissions;
            AccessExpiry = accessExpiry;
        }

        /// <summary>
        /// List of permissions required for Access URL
        /// </summary>
        public List<AccessPermission> AccessPermissions { get; set; }

        /// <summary>
        /// Expiry time of the Access URL
        /// </summary>
        public DateTimeOffset? AccessExpiry { get; set; }
    }
}
