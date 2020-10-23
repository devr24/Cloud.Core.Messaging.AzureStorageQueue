namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using Cloud.Core;
    using Cloud.Core.Messaging.AzureStorageQueue;
    using Cloud.Core.Messaging.AzureStorageQueue.Config;
    using Cloud.Core.Messaging.AzureStorageQueue.Models;

    /// <summary>
    /// Class Service Collection extensions.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add Storage Queue singleton of type T, using named properties (as opposed to passing MsiConfig/ServicePrincipleConfig etc).
        /// Will automatically use MsiConfiguration.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="services">Service collection to extend</param>
        /// <param name="instanceName">Instance name of Storage Queue.</param>
        /// <param name="tenantId">Tenant Id where Storage Queue exists.</param>
        /// <param name="subscriptionId">Subscription within the tenancy to use for the Storage Queue instance.</param>
        /// <param name="receiver">Receiver configuration (if any).</param>
        /// <param name="sender">Sender configuration (if any).</param>
        /// <returns>Modified service collection with the IReactiveMessenger, IMessenger and NamedInstanceFactory{T} configured.</returns>
        public static IServiceCollection AddStorageQueueSingleton<T>(this IServiceCollection services, string instanceName, string tenantId, string subscriptionId, ReceiverConfig receiver = null, SenderConfig sender = null)
            where T : IMessageOperations
        {
            return services.AddStorageQueueSingletonNamed<T>(null, instanceName, tenantId, subscriptionId, receiver, sender);
        }

        /// <summary>
        /// Add Storage Queue singleton of type T, using named properties (as opposed to passing MsiConfig/ServicePrincipleConfig etc).
        /// Will automatically use MsiConfiguration.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="services">Service collection to extend</param>
        /// <param name="key">Key to identify the named instance of the Storage Queue singleton.</param>
        /// <param name="instanceName">Instance name of Storage Queue.</param>
        /// <param name="tenantId">Tenant Id where Storage Queue exists.</param>
        /// <param name="subscriptionId">Subscription within the tenancy to use for the Storage Queue instance.</param>
        /// <param name="receiver">Receiver configuration (if any).</param>
        /// <param name="sender">Sender configuration (if any).</param>
        /// <returns>Modified service collection with the IReactiveMessenger, IMessenger and NamedInstanceFactory{T} configured.</returns>
        public static IServiceCollection AddStorageQueueSingletonNamed<T>(this IServiceCollection services, string key, string instanceName, string tenantId, string subscriptionId, ReceiverConfig receiver = null, SenderConfig sender = null)
            where T : IMessageOperations
        {
            var storageQueueInstance = new StorageQueueMessenger(new MsiConfig
            {
                InstanceName = instanceName,
                TenantId = tenantId,
                SubscriptionId = subscriptionId,
                Receiver = receiver,
                Sender = sender
            });

            if (!key.IsNullOrEmpty())
            {
                storageQueueInstance.Name = key;
            }

            services.AddSingleton(typeof(T), storageQueueInstance);
            services.AddFactoryIfNotAdded<IReactiveMessenger>();
            services.AddFactoryIfNotAdded<IMessenger>();
            return services;
        }

        /// <summary>
        /// Adds the Storage Queue singleton.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="services">The services.</param>
        /// <param name="config">The configuration.</param>
        /// <returns>ServiceCollection.</returns>
        /// <exception cref="InvalidOperationException">Problem occurred while configuring Storage Queue Manager Identify config</exception>
        public static IServiceCollection AddStorageQueueSingleton<T>(this IServiceCollection services, MsiConfig config)
            where T : IMessageOperations
        {
            var storageQueueInstance = new StorageQueueMessenger(config);
            services.AddSingleton(typeof(T), storageQueueInstance);
            services.AddFactoryIfNotAdded<IReactiveMessenger>();
            services.AddFactoryIfNotAdded<IMessenger>();
            return services;
        }

        /// <summary>
        /// Adds the Storage Queue singleton.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="services">The services.</param>
        /// <param name="config">The configuration.</param>
        /// <returns>IServiceCollection.</returns>
        public static IServiceCollection AddStorageQueueSingleton<T>(this IServiceCollection services, ConnectionConfig config)
            where T : IMessageOperations
        {
            var storageQueueInstance = new StorageQueueMessenger(config);
            services.AddSingleton(typeof(T), storageQueueInstance);
            services.AddFactoryIfNotAdded<IReactiveMessenger>();
            services.AddFactoryIfNotAdded<IMessenger>();
            return services;
        }

        /// <summary>
        /// Adds the Storage Queue singleton.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="services">The services.</param>
        /// <param name="config">The configuration.</param>
        /// <returns>IServiceCollection.</returns>
        public static IServiceCollection AddStorageQueueSingleton<T>(this IServiceCollection services, ServicePrincipleConfig config)
            where T : IMessageOperations
        {
            var storageQueueInstance = new StorageQueueMessenger(config);
            services.AddSingleton(typeof(T), storageQueueInstance);
            services.AddFactoryIfNotAdded<IReactiveMessenger>();
            services.AddFactoryIfNotAdded<IMessenger>();
            return services;
        }
    }
}
