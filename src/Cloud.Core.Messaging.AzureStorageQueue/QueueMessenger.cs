namespace Cloud.Core.Messaging.AzureStorageQueue
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Threading;
    using System.Threading.Tasks;
    using Comparer;
    using Config;
    using Models;
    using Microsoft.Azure.Storage.Queue;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    /// <summary>
    /// Azure Storage Queue specific implementation of IMessenger and IReactiveMessenger.
    /// Implements the <see cref="IMessenger" />
    /// Implements the <see cref="IReactiveMessenger" />
    /// </summary>
    /// <seealso cref="IMessenger" />
    /// <seealso cref="IReactiveMessenger" />
    /// <seealso cref="StorageQueueBase" />
    public class QueueMessenger : StorageQueueBase, IMessenger, IReactiveMessenger
    {
        #region Construction

        private StorageQueueEntityManager _entityManager;
        internal bool Disposed;
        internal readonly object CancelGate = new object();
        internal readonly object ReceiveGate = new object();
        internal readonly ISubject<object> MessagesIn = new Subject<object>();

        internal readonly ConcurrentDictionary<object, object> Messages = new ConcurrentDictionary<object, object>(ObjectReferenceEqualityComparer<object>.Default);
        internal readonly ConcurrentDictionary<Type, Timer> LockTimers = new ConcurrentDictionary<Type, Timer>(ObjectReferenceEqualityComparer<Type>.Default);

        /// <summary>
        /// Property representing IManager interface method.
        /// </summary>
        public IMessageEntityManager EntityManager
        {
            get
            {
                if (_entityManager == null)
                {
                    _entityManager = new StorageQueueEntityManager(ReceiverQueue, SenderQueue, CloudQueueClient);
                }

                return _entityManager;
            }
        }

        /// <summary>
        /// Initializes a new instance of ServiceBusMessenger with Managed Service Identity (MSI) authentication.
        /// </summary>
        /// <param name="config">The Msi ServiceBus configuration.</param>
        /// <param name="logger">The logger.</param>
        public QueueMessenger([NotNull]MsiConfig config, ILogger logger = null)
            : base(config, logger)
        {
        }

        /// <summary>
        /// Initializes a new instance of ServiceBusMessenger with Service Principle authentication.
        /// </summary>
        /// <param name="config">The Service Principle configuration.</param>
        /// <param name="logger">The logger.</param>
        public QueueMessenger([NotNull]ServicePrincipleConfig config, ILogger logger = null)
            : base(config, logger)
        {
        }

        /// <summary>
        /// Initializes a new instance of the ServiceBusMessenger using a connection string.
        /// </summary>
        /// <param name="config">The connection string configuration.</param>
        /// <param name="logger">The logger.</param>
        public QueueMessenger([NotNull]ConnectionConfig config, ILogger logger = null)
            : base(config, logger)
        {
        }

        #endregion

        #region Send Messages

        /// <summary>
        /// Sends message to the storage queue
        /// </summary>
        /// <typeparam name="T">Type of object on the entity.</typeparam>
        /// <param name="message">The message body to be sent.</param>
        /// <returns>Task.</returns>
        public async Task Send<T>(T message) where T : class
        {
            await Send(message, null);
        }

        /// <summary>
        /// Sends a message to storage queue with properties.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message">The message body to be sent.</param>
        /// <param name="properties">The properties of the message.</param>
        /// <returns>Task.</returns>
        public async Task Send<T>(T message, KeyValuePair<string, object>[] properties) where T : class
        {
            var obj = new MessageWrapper<T>(message, properties);
            var outgoing = new CloudQueueMessage(obj.AsJson());

            const long maxAllowedSizeBytes = 64 * 1024;
            var messageLengthBytes = outgoing.AsBytes.Length;

            if (messageLengthBytes > maxAllowedSizeBytes)
            {
                throw new ArgumentOutOfRangeException(nameof(message), $"Max message size of {maxAllowedSizeBytes} bytes exceeded (message size was {messageLengthBytes})");
            }

            await SenderQueue.AddMessageAsync(outgoing);
        }

        /// <summary>
        /// Send a batch of messages to storage queue.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="messages">List of messages to send.</param>
        /// <param name="batchSize">Size of message batches to send in a single call. If set to zero, uses the default batch size from config.</param>
        /// <returns>Task.</returns>
        public async Task SendBatch<T>(IEnumerable<T> messages, int batchSize = 10) where T : class
        {
            await SendBatch(messages, (KeyValuePair<string, object>[])null, batchSize);
        }

        /// <summary>
        /// Sends a message to storage queue with properties.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="messages">List of messages to send</param>
        /// <param name="properties">The properties applied to all messages</param>
        /// <param name="batchSize">Size of message batches to send in a single call. If set to zero, uses the default batch size from config.</param>
        /// <returns>Task.</returns>
        public async Task SendBatch<T>(IEnumerable<T> messages, KeyValuePair<string, object>[] properties, int batchSize = 100) where T : class
        {
            Func<T, KeyValuePair<string, object>[]> func = (a) => properties;
            if (properties == null)
                func = null;

            await SendBatch(messages, func, batchSize);
        }

        /// <summary>
        /// Send a batch of messages to storage queue along with a function to set the message properties.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="messages">List of messages to send</param>
        /// <param name="setProps">Function to set the properties for each message in the batch.</param>
        /// <param name="batchSize">Size of message batches to send in a single call. If set to zero, uses the default batch size from config.</param>
        /// <returns>Task.</returns>
        public async Task SendBatch<T>(IEnumerable<T> messages, Func<T, KeyValuePair<string, object>[]> setProps, int batchSize = 100) where T : class
        {
            foreach (var msg in messages)
            {
                await Send(msg);
            }
        }

        #endregion

        #region Receive Messages

        /// <summary>
        /// Read a single message.
        /// </summary>
        /// <typeparam name="T">Type of object on the entity.</typeparam>
        /// <returns>IMessageItem&lt;T&gt;.</returns>
        public T ReceiveOne<T>() where T : class
        {
            var message = ReceiveOneEntity<T>();
            return message?.Body;
        }

        public IMessageEntity<T> ReceiveOneEntity<T>() where T : class
        {
            Monitor.Enter(ReceiveGate);
            try
            {
                return GetMessages<T>(1).FirstOrDefault();
            }
            finally
            {
                Monitor.Exit(ReceiveGate);
            }
        }

        private List<MessageWrapper<T>> GetMessages<T>(int batchSize = 10)
            where T : class
        {
            var messages = ReceiverQueue.GetMessagesAsync(batchSize).GetAwaiter().GetResult().ToList();
            var msgItems = new List<MessageWrapper<T>>();

            if (messages.Count > 0)
            {
                foreach (var message in messages)
                {
                    var result = GetMessageBody<T>(message);

                    if (!Messages.ContainsKey(result.Body))
                    {
                        Messages[result.Body] = result;
                        msgItems.Add(result);
                    }
                }
            }

            return msgItems;
        }

        private MessageWrapper<T> GetMessageBody<T>(CloudQueueMessage message)
            where T : class
        {
            var result = JsonConvert.DeserializeObject<MessageWrapper<T>>(message.AsString);

            if (result.IsEmpty())
            {
                var messageObject = JsonConvert.DeserializeObject<T>(message.AsString);
                result = new MessageWrapper<T>(messageObject);
            }

            result.OriginalMessage = message;
            return result;
        }

        private bool _inBatch;

        /// <summary>
        /// Starts the receive.
        /// </summary>
        /// <typeparam name="T">Type of object to receive</typeparam>
        /// <param name="batchSize">Size of message batches to receive in a single call. If set to zero, uses the default batch size from config.</param>
        /// <returns>IObservable{T}.</returns>
        public IObservable<T> StartReceive<T>(int batchSize = 10) where T : class
        {
            IObserver<T> obs = MessagesIn.AsObserver();

            LockTimers.TryAdd(typeof(T), new Timer(t =>
            {
                if (!_inBatch)
                {
                    _inBatch = true;

                    try
                    {
                        var msgItems = GetMessages<T>(batchSize);

                        foreach (var msg in msgItems)
                        {
                            obs.OnNext(msg.Body);
                        }
                    }
                    catch (Exception e)
                    {
                        obs.OnError(e);
                    }
                    finally
                    {
                        _inBatch = false;
                    }
                }
            }, null, TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(500)));

            return MessagesIn.OfType<T>().AsObservable();
        }

        /// <summary>
        /// Cancels the receive of messages.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void CancelReceive<T>() where T : class
        {
            Monitor.Enter(CancelGate);

            try
            {
                // Remove specific existing subscriptions
                LockTimers.TryRemove(typeof(T), out var msgTimer);
                msgTimer?.Dispose();

            }
            catch
            {
                // do nothing here...
            }
            finally
            {
                Monitor.Exit(CancelGate);
            }
        }

        /// <summary>
        /// Receives the specified success callback.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="successCallback">Callback to execute after a message batch has been received.</param>
        /// <param name="errorCallback">Callback to execute after an error occurs.</param>
        /// <param name="batchSize">Size of message batches to receive in a single call. If set to zero, uses the default batch size from config.</param>
        /// <exception cref="InvalidOperationException">Callback for this message type already configured. Only one callback per type is supported.</exception>
        public void Receive<T>(Action<T> successCallback, Action<Exception> errorCallback, int batchSize = 10) where T : class
        {
            Monitor.Enter(ReceiveGate);

            try
            {
                LockTimers.TryAdd(typeof(T), new Timer(t =>
                {
                    if (!_inBatch)
                    {
                        _inBatch = true;

                        try
                        {
                            var msgItems = GetMessages<T>(batchSize);

                            foreach (var msg in msgItems)
                            {
                                successCallback(msg.Body);
                            }
                        }
                        catch (Exception e)
                        {
                            errorCallback(e);
                        }
                        finally
                        {
                            _inBatch = false;
                        }
                    }
                }, null, TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(500)));
            }
            finally
            {
                Monitor.Exit(ReceiveGate);
            }
        }

        /// <summary>
        /// Read message properties for the passed message.
        /// </summary>
        /// <typeparam name="T">Type of message body.</typeparam>
        /// <typeparam name="O">Type of properties.</typeparam>
        /// <param name="msg">>Message body, used to identify actual storage queue message.</param>
        /// <returns>IDictionary&lt;System.String, System.Object&gt;.</returns>
        public O ReadProperties<T, O>(T msg)
            where T : class
            where O : class, new()
        {
            var results = (MessageWrapper<T>)Messages[msg];
            return results.GetPropertiesTyped<O>();
        }

        /// <summary>
        /// Read message properties for the passed message.
        /// </summary>
        /// <typeparam name="T">Type of message body.</typeparam>
        /// <param name="msg">Message body, used to identify actual storage queue message.</param>
        /// <returns>IDictionary&lt;System.String, System.Object&gt;.</returns>
        public IDictionary<string, object> ReadProperties<T>(T msg) where T : class
        {
            var results = (MessageWrapper<T>)Messages[msg];
            return results.Properties;
        }

        #endregion

        /// <summary>
        /// Completes the specified message.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message">The message.</param>
        /// <returns>Task.</returns>
        /// <inheritdoc />
        public async Task Complete<T>(T message) where T : class
        {
            var sourceMessage = (MessageWrapper<T>)Messages[message];

            await ReceiverQueue.DeleteMessageAsync(sourceMessage.OriginalMessage);
            Messages.TryRemove(message, out _);
        }

        public Task CompleteAll<T>(IEnumerable<T> messages) where T : class
        {
            foreach (var msg in messages)
            {
                Task.FromResult(Messages.TryRemove(msg, out _));
            }

            return Task.FromResult(true);
        }

        /// <summary>
        /// Abandons the specified message.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message">The message.</param>
        /// <returns>Task.</returns>
        /// <inheritdoc />
        public async Task Abandon<T>(T message) where T : class
        {
            var msg = (MessageWrapper<T>)Messages[message];
            await ReceiverQueue.UpdateMessageAsync(msg.OriginalMessage, TimeSpan.FromSeconds(10),
                MessageUpdateFields.Content | MessageUpdateFields.Visibility);

            Messages.TryRemove(message, out _);
        }

        /// <summary>
        /// Errors the specified message.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message">The message.</param>
        /// <param name="reason">The reason.</param>
        /// <returns>Task.</returns>
        /// <inheritdoc />
        public async Task Error<T>(T message, string reason = null) where T : class
        {
            try
            {
                await Complete(message);
            }
            finally
            {
                await Task.FromResult(Messages.TryRemove(message, out _));
            }
        }

        /// <summary>
        /// Get a signed access url using supplied permissions and expiry
        /// </summary>
        /// <param name="signedAccessConfig">Config object with required access permissions and expiry</param>
        /// <returns></returns>
        public string GetSignedAccessUrl(ISignedAccessConfig signedAccessConfig)
        {
            var queuePolicyPermissions = GetAzureAccessQueuePolicyPermissions(signedAccessConfig.AccessPermissions);

            var accessPolicy = new SharedAccessQueuePolicy
            {
                Permissions = queuePolicyPermissions,
                SharedAccessExpiryTime = signedAccessConfig.AccessExpiry
            };

            var queueSignature = ReceiverQueue.GetSharedAccessSignature(accessPolicy);

            var queueAccessUrl = ReceiverQueue.Uri + queueSignature;

            return queueAccessUrl;
        }

        /// <summary>
        /// Update the receiver to listen to a different storage Queue
        /// </summary>
        /// <param name="entityName">The name of the updated storage queue to listen to.</param>
        /// <param name="entitySubscriptionName">Not required for Azure Storage Queues.</param>
        /// <param name="createIfNotExists">Creates the entity if it does not exist. Not needed for Storage Queue right now</param>
        /// <param name="entityFilter">A filter that will be applied to the entity if created through this method. Not Needed for Storage Queue right now.</param>
        /// <param name="supportStringBodyType">Read message body as a string. Not Needed for Storage Queue right now.</param>
        /// <returns></returns>
        public Task UpdateReceiver(string entityName, string entitySubscriptionName = null, bool createIfNotExists = false, KeyValuePair<string, string>? entityFilter = default, bool supportStringBodyType = false)
        {
            if (Config.Receiver == null)
            {
                Config.Receiver = new ReceiverSetup()
                {
                    EntityName = entityName,
                    CreateEntityIfNotExists = createIfNotExists
                };
            }
            else
            {
                Config.Receiver.EntityName = entityName;

            }

            //To make it get re-configured next time it's requested
            ReceiverQueue = null;

            return Task.CompletedTask;
        }

        /// <summary>
        /// Method to translate the generic permissions required to concrete Azure Queue Access Permissions
        /// </summary>
        /// <param name="requiredAccessPermissions">Method to translate the generic permissions required to concrete Azure Queue Access Permissions</param>
        /// <returns>Enum of Azure Queue Permissions with bitwise operator</returns>
        internal SharedAccessQueuePermissions GetAzureAccessQueuePolicyPermissions(List<AccessPermission> requiredAccessPermissions)
        {
            var azurePermissions = new List<SharedAccessQueuePermissions>();

            //We have no direct generic mapping to Process Messages but based on Microsoft Documentation, having both list and Delete should map to ProcessMessages
            if (requiredAccessPermissions.Contains(AccessPermission.List) && requiredAccessPermissions.Contains(AccessPermission.Delete))
            {
                azurePermissions.Add(SharedAccessQueuePermissions.ProcessMessages);
            }

            foreach (var permission in requiredAccessPermissions)
            {
                switch (permission)
                {
                    case AccessPermission.None:
                        azurePermissions.Add(SharedAccessQueuePermissions.None);
                        break;
                    case AccessPermission.Add:
                    case AccessPermission.Create:
                        azurePermissions.Add(SharedAccessQueuePermissions.Add);
                        break;
                    case AccessPermission.Update:
                    case AccessPermission.Write:
                        azurePermissions.Add(SharedAccessQueuePermissions.Update);
                        break;
                    case AccessPermission.Read:
                        azurePermissions.Add(SharedAccessQueuePermissions.Read);
                        break;
                }
            }

            //Ensure that if no permissions are passed through, that we default the permissions list to None
            if (!azurePermissions.Any())
            {
                azurePermissions.Add(SharedAccessQueuePermissions.None);
            }

            var blobPolicyPermissions = azurePermissions.Aggregate((x, y) => x | y);
            return blobPolicyPermissions;
        }


        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <inheritdoc />
        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);

            // Suppress finalization.
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (Disposed)
                return;

            if (disposing)
            {
                // Clear all message lock timers and messages.
                LockTimers.Release();
                Messages.Clear();
            }

            Disposed = true;
        }
    }
}
