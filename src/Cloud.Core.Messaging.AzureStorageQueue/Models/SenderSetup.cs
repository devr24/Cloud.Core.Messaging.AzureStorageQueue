namespace Cloud.Core.Messaging.AzureStorageQueue.Models
{
    using System.Text;
    using System.ComponentModel.DataAnnotations;
    using Validation;

    /// <summary>
    /// Sender setup information, used when creating a connection to send messages to an entity.
    /// </summary>
    public class SenderSetup: AttributeValidator
    {
        private string _entityName;

        /// <summary>
        /// Gets or sets the name of the entity to send to.
        /// </summary>
        /// <value>The name of the entity.</value>
        [Required]
        public string EntityName
        {
            get => _entityName;
            set => _entityName = value.ToLowerInvariant();
        }

        /// <summary>
        /// Gets or sets a value indicating whether to [create entity if it does not already exist].
        /// </summary>
        /// <value><c>true</c> if [create entity if not exists]; otherwise, <c>false</c> (don't auto create).</value>
        public bool CreateEntityIfNotExists { get; set; }

        /// <summary>
        /// Gets the maximum entity size kb.
        /// </summary>
        /// <value>The maximum entity size kb.</value>
        public long MaxMessageSizeKb => MaxMessageSizeBytes / 1000;
        /// <summary>
        /// Gets the maximum entity size bytes.
        /// </summary>
        /// <value>The maximum entity size bytes.</value>
        public long MaxMessageSizeBytes => 64000;

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="string" /> that represents this instance.</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append(" EntityName: ");
            sb.AppendLine(EntityName);
            sb.Append("MaxMessageSizeKb: ");
            sb.AppendLine(MaxMessageSizeKb.ToString());
            sb.Append(" CreateEntityIfNotExists: ");
            sb.AppendLine(CreateEntityIfNotExists.ToString());

            return sb.ToString();
        }
    }
}
