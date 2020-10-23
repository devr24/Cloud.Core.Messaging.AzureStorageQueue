namespace Cloud.Core.Messaging.AzureStorageQueue.Models
{
    using System.Globalization;
    using System.Text;
    using System.ComponentModel.DataAnnotations;
    using Validation;

    /// <summary>
    /// Receiver setup information, used when creating a connection to listen to messages from an entity.
    /// </summary>
    public class ReceiverConfig : AttributeValidator
    {
        private string _entityName;

        /// <summary>
        /// Gets or sets the name of the entity to receive from.
        /// </summary>
        /// <value>The name of the entity to receive from.</value>
        [Required]
        public string EntityName
        {
            get => _entityName;
            set => _entityName = value.ToLowerInvariant();
        }

        /// <summary>
        /// Remove messages which fail to serialize.  For cases where one storage queue is used for one model type.
        /// Set this to False in cases where you want a queue to handle multiple message types.
        /// </summary>
        public bool RemoveSerializationFailureMessages { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to [create the receiver entity if it does not already exist].
        /// </summary>
        /// <value><c>true</c> if [create entity if not exists]; otherwise, <c>false</c> (don't auto create).</value>
        public bool CreateEntityIfNotExists { get; set; }

        /// <summary>
        /// Gets or sets the occurence of "polling" in seconds (how often the message receiver queries Storage Queue for new messages).
        /// </summary>
        /// <value>The poll frequency in seconds.</value>
        public double PollFrequencyInSeconds { get; set; } = 0.05;

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="string" /> that represents this instance.</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append(" EntityName: ");
            sb.AppendLine(EntityName);
            sb.Append(" CreateEntityIfNotExists: ");
            sb.AppendLine(CreateEntityIfNotExists.ToString(CultureInfo.InvariantCulture));
            sb.Append(" PollFrequencyInSeconds: ");
            sb.Append(PollFrequencyInSeconds.ToString(CultureInfo.InvariantCulture));

            return sb.ToString();
        }
    }
}
