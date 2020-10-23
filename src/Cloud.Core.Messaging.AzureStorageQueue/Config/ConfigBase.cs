namespace Cloud.Core.Messaging.AzureStorageQueue.Config
{
    using System;
    using Models;
    using Validation;

    /// <summary>
    /// Configuration Base class, used with each of the individual config classes.
    /// </summary>
    public abstract class ConfigBase : AttributeValidator
    {
        /// <summary>
        /// Gets or sets the receiver configuration.
        /// </summary>
        /// <value>The receiver config.</value>
        public ReceiverConfig Receiver { get; set; }

        /// <summary>
        /// Gets or sets the sender configuration.
        /// </summary>
        /// <value>The sender config.</value>
        public SenderConfig Sender { get; set; }

        /// <summary>
        /// Validates this instance.
        /// </summary>
        public override ValidateResult Validate(IServiceProvider serviceProvider = null)
        {
            // Validate receiver config if set.
            Receiver?.ThrowIfInvalid();

            // Validate the sender config if its been set.
            Sender?.ThrowIfInvalid();

            return base.Validate(serviceProvider);
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="string" /> that represents this instance.</returns>
        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Environment.NewLine}ReceiverConfig: {(Receiver == null ? "[NOT SET]" : Receiver.ToString())}"+
                $"{Environment.NewLine}SenderConfig: {(Sender == null ? "[NOT SET]" : Sender.ToString())}";
        }
    }
}
