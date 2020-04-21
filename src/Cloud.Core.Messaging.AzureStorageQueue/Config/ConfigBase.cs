namespace Cloud.Core.Messaging.AzureStorageQueue.Config
{
    using System;
    using Models;

    /// <summary>
    /// Configuration Base class, used with each of the individual config classes.
    /// </summary>
    public abstract class ConfigBase
    {
        /// <summary>
        /// Gets or sets the receiver configuration.
        /// </summary>
        /// <value>The receiver config.</value>
        public ReceiverSetup Receiver { get; set; }

        /// <summary>
        /// Gets or sets the sender configuration.
        /// </summary>
        /// <value>The sender config.</value>
        public SenderSetup Sender { get; set; }

        /// <summary>
        /// Validates this instance.
        /// </summary>
        public virtual void Validate()
        {
            // Validate receiver config if set.
            Receiver?.Validate();

            // Validate the sender config if its been set.
            Sender?.Validate();
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="string" /> that represents this instance.</returns>
        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Environment.NewLine}ReceiverSetup: {(Receiver == null ? "[NOT SET]" : Receiver.ToString())}"+
                $"{Environment.NewLine}SenderSetup: {(Sender == null ? "[NOT SET]" : Sender.ToString())}";
        }
    }
}
