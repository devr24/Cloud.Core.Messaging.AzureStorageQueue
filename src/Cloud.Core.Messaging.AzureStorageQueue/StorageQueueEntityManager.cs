using System;
using System.Threading.Tasks;
using Microsoft.Azure.Storage.Queue;

namespace Cloud.Core.Messaging.AzureStorageQueue
{
    /// <summary>
    /// Methods to manage the entity level details of storage queues 
    /// </summary>
    /// <seealso cref="Cloud.Core.IMessageEntityManager" />
    public class StorageQueueEntityManager : IMessageEntityManager
    {
        private readonly CloudQueue _receiverQueue;
        private readonly CloudQueue _senderQueue;
        private readonly CloudQueueClient _queueClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="StorageQueueEntityManager"/> class.
        /// </summary>
        /// <param name="receiverQueue">The receiver queue.</param>
        /// <param name="senderQueue">The sender queue.</param>
        /// <param name="queueClient">The Storage Queue client.</param>
        internal StorageQueueEntityManager(CloudQueue receiverQueue, CloudQueue senderQueue, CloudQueueClient queueClient)
        {
            _receiverQueue = receiverQueue;
            _senderQueue = senderQueue;
            _queueClient = queueClient;
        }

        /// <summary>
        /// This has not been implemented
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task<decimal> GetReceiverEntityUsagePercentage()
        {
            //Max Queue Size is 500TB (or 2PB) https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-azure-and-service-bus-queues-compared-contrasted#capacity-and-quotas
            // And we need MSI auth to access this metric so we can't implement it in a way that it can be read from the Data Plane. Only the Control Plane
            throw new NotImplementedException();
        }

        /// <summary>
        /// This has not been implemented
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task<decimal> GetSenderEntityUsagePercentage()
        {
            // Max Queue Size is 500TB (or 2PB) https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-azure-and-service-bus-queues-compared-contrasted#capacity-and-quotas
            // And we need MSI auth to access this metric so we can't implement it in a way that it can be read from the Data Plane. Only the Control Plane
            throw new NotImplementedException();
        }

        /// <summary>
        /// Determines whether Receiver entity is disabled.
        /// </summary>
        /// <returns></returns>
        public Task<bool> IsReceiverEntityDisabled()
        {
            return Task.FromResult(false);
        }

        /// <summary>
        /// Determines whether Sender entity is disabled.
        /// </summary>
        /// <returns></returns>
        public Task<bool> IsSenderEntityDisabled()
        {
            return Task.FromResult(false);
        }

        /// <summary>
        /// Gets the receiver message count.
        /// </summary>
        /// <returns></returns>
        public async Task<EntityMessageCount> GetReceiverMessageCount()
        {
            await _receiverQueue.FetchAttributesAsync();
            return new EntityMessageCount { ActiveEntityCount = _receiverQueue.ApproximateMessageCount.GetValueOrDefault(0) };
        }

        /// <summary>
        /// Gets the sender message count.
        /// </summary>
        /// <returns></returns>
        public async Task<EntityMessageCount> GetSenderMessageCount()
        {
            await _senderQueue.FetchAttributesAsync();
            return new EntityMessageCount { ActiveEntityCount = _senderQueue.ApproximateMessageCount.GetValueOrDefault(0) };
        }

        /// <summary>
        /// Creates a Storage Queue with the given config
        /// </summary>
        /// <param name="config">The configuration.</param>
        public async Task CreateEntity(IEntityConfig config)
        {
            await _queueClient.GetQueueReference(config.EntityName).CreateIfNotExistsAsync();

            // Short delay to allow it to finish creating.
            await Task.Delay(2000);
        }

        /// <summary>
        /// Deletes the queue entity.
        /// </summary>
        /// <param name="entityName">The entity name to delete.</param>
        public async Task DeleteEntity(string entityName)
        {
            entityName = entityName.ToLowerInvariant();
            await _queueClient.GetQueueReference(entityName).DeleteIfExistsAsync();

            // Short delay to allow it to finish deleting.
            await Task.Delay(2000);
        }

        /// <summary>
        /// Check the queue entity exists.
        /// </summary>
        /// <param name="entityName">The entity name to check exists.</param>
        public async Task<bool> EntityExists(string entityName)
        {
            entityName = entityName.ToLowerInvariant();
            return await _queueClient.GetQueueReference(entityName).ExistsAsync();
        }
    }
}
