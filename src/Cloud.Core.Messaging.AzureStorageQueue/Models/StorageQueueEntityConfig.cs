namespace Cloud.Core.Messaging.AzureStorageQueue.Models
{
    /// <summary>
    /// Configuration required to create a Storage Queue 
    /// </summary>
    /// <seealso cref="Cloud.Core.IEntityConfig" />
    public class StorageQueueEntityConfig : IEntityConfig
    {
        private string _entityName;

        /// <summary>
        /// Gets or sets the name of the storage queue that will be created.
        /// </summary>
        /// <value>
        /// The name of the storage queue.
        /// </value>
        public string EntityName { get => _entityName; set => _entityName = value?.ToLowerInvariant(); }
    }
}
