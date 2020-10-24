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
        /// Add Storage Queue singleton of type StorageQueueMessenger, using named properties (as opposed to passing MsiConfig/ServicePrincipleConfig etc).
        /// Will automatically use MsiConfiguration.
        /// </summary>
        /// <param name="services">Service collection to extend</param>
        /// <param name="instanceName">Instance name of Storage Queue.</param>
        /// <param name="tenantId">Tenant Id where Storage Queue exists.</param>
        /// <param name="subscriptionId">Subscription within the tenancy to use for the Storage Queue instance.</param>
        /// <param name="receiver">Receiver configuration (if any).</param>
        /// <param name="sender">Sender configuration (if any).</param>
        /// <returns>Modified service collection with the StorageQueueMessenger and NamedInstanceFactory{StorageQueueMessenger} configured.</returns>
        public static IServiceCollection AddStorageQueueSingleton(this IServiceCollection services, string instanceName, string tenantId, string subscriptionId, ReceiverConfig receiver = null, SenderConfig sender = null)
        {
            return services.AddStorageQueueSingletonNamed<StorageQueueMessenger>(null, instanceName, tenantId, subscriptionId, receiver, sender);
        }

        /// <summary>
        /// Add Storage Queue singleton of type T, using named properties (as opposed to passing MsiConfig/ServicePrincipleConfig etc).
        /// Will automatically use MsiConfiguration.
        /// </summary>
        /// <typeparam name="T">Type of IMessageOperation (IMessenger or IReactiveMessenger).</typeparam>
        /// <param name="services">Service collection to extend.</param>
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
            return AddNamedInstance<T>(services, key, new StorageQueueMessenger(new MsiConfig
            {
                InstanceName = instanceName,
                TenantId = tenantId,
                SubscriptionId = subscriptionId,
                Receiver = receiver,
                Sender = sender
            }));
        }

        /// <summary>
        /// Add Storage Queue singleton of type StorageQueueMessenger, using named properties (as opposed to passing MsiConfig/ServicePrincipleConfig etc).
        /// Will automatically use MsiConfiguration.
        /// </summary>
        /// <param name="services">Service collection to extend.</param>
        /// <param name="key">Key to identify the named instance of the Storage Queue singleton.</param>
        /// <param name="instanceName">Instance name of Storage Queue.</param>
        /// <param name="tenantId">Tenant Id where Storage Queue exists.</param>
        /// <param name="subscriptionId">Subscription within the tenancy to use for the Storage Queue instance.</param>
        /// <param name="receiver">Receiver configuration (if any).</param>
        /// <param name="sender">Sender configuration (if any).</param>
        /// <returns>Modified service collection with the StorageQueueMessenger and NamedInstanceFactory{StorageQueueMessenger} configured.</returns>
        public static IServiceCollection AddStorageQueueSingletonNamed(this IServiceCollection services, string key, string instanceName, string tenantId, string subscriptionId, ReceiverConfig receiver = null, SenderConfig sender = null)
        {
            return AddNamedInstance<StorageQueueMessenger>(services, key, new StorageQueueMessenger(new MsiConfig
            {
                InstanceName = instanceName,
                TenantId = tenantId,
                SubscriptionId = subscriptionId,
                Receiver = receiver,
                Sender = sender
            }));
        }

        /// <summary>
        /// Adds the service bus singleton instance and NamedInstanceFactory{ServiceBusInstance} configured.
        /// Uses MsiConfig to setup configuration.
        /// </summary>
        /// <typeparam name="T">Type of IMessageOperation (IMessenger or IReactiveMessenger).</typeparam>
        /// <param name="services">Service collection to extend.</param>
        /// <param name="config">The configuration.</param>
        /// <returns>Modified service collection with the IMessenger or IReactiveMessenger and NamedInstanceFactory{T} configured.</returns>
        public static IServiceCollection AddStorageQueueSingleton<T>(this IServiceCollection services, MsiConfig config)
            where T : IMessageOperations
        {
            return AddNamedInstance<T>(services, null, new StorageQueueMessenger(config));
        }

        /// <summary>
        /// Adds the service bus singleton instance and NamedInstanceFactory{ServiceBusInstance} configured.
        /// Uses ConnectionConfig to setup configuration.
        /// </summary>
        /// <typeparam name="T">Type of IMessageOperation (IMessenger or IReactiveMessenger).</typeparam>
        /// <param name="services">Service collection to extend.</param>
        /// <param name="config">The configuration.</param>
        /// <returns>Modified service collection with the IMessenger or IReactiveMessenger and NamedInstanceFactory{T} configured.</returns>
        public static IServiceCollection AddStorageQueueSingleton<T>(this IServiceCollection services, ConnectionConfig config)
            where T : IMessageOperations
        {
            return AddNamedInstance<T>(services, null, new StorageQueueMessenger(config));
        }

        /// <summary>
        /// Adds the service bus singleton instance and NamedInstanceFactory{ServiceBusInstance} configured.
        /// Uses ServicePrincipleConfig to setup configuration.
        /// </summary>
        /// <typeparam name="T">Type of IMessageOperation (IMessenger or IReactiveMessenger).</typeparam>
        /// <param name="services">Service collection to extend.</param>
        /// <param name="config">The configuration.</param>
        /// <returns>Modified service collection with the IMessenger or IReactiveMessenger and NamedInstanceFactory{T} configured.</returns>
        public static IServiceCollection AddStorageQueueSingleton<T>(this IServiceCollection services, ServicePrincipleConfig config)
            where T : IMessageOperations
        {
            return AddNamedInstance<T>(services, null, new StorageQueueMessenger(config));
        }

        /// <summary>
        /// Adds the service bus singleton instance and NamedInstanceFactory{ServiceBusInstance} configured.
        /// Uses MsiConfig to setup configuration.
        /// </summary>
        /// <param name="services">Service collection to extend.</param>
        /// <param name="config">The configuration.</param>
        /// <returns>Modified service collection with the ServiceBusMessenger and NamedInstanceFactory{StorageQueueMessenger} configured.</returns>
        public static IServiceCollection AddStorageQueueSingleton(this IServiceCollection services, MsiConfig config)
        {
            return AddNamedInstance<StorageQueueMessenger>(services, null, new StorageQueueMessenger(config));
        }

        /// <summary>
        /// Adds the service bus singleton instance and NamedInstanceFactory{ServiceBusInstance} configured.
        /// Uses ConnectionConfig to setup configuration.
        /// </summary>
        /// <param name="services">Service collection to extend.</param>
        /// <param name="config">The configuration.</param>
        /// <returns>Modified service collection with the ServiceBusMessenger and NamedInstanceFactory{StorageQueueMessenger} configured.</returns>
        public static IServiceCollection AddStorageQueueSingleton(this IServiceCollection services, ConnectionConfig config)
        {
            return AddNamedInstance<StorageQueueMessenger>(services, null, new StorageQueueMessenger(config));
        }

        /// <summary>
        /// Adds the service bus singleton instance and NamedInstanceFactory{ServiceBusInstance} configured.
        /// Uses ServicePrincipleConfig to setup configuration.
        /// </summary>
        /// <param name="services">Service collection to extend.</param>
        /// <param name="config">The configuration.</param>
        /// <returns>Modified service collection with the ServiceBusMessenger and NamedInstanceFactory{StorageQueueMessenger} configured.</returns>
        public static IServiceCollection AddStorageQueueSingleton(this IServiceCollection services, ServicePrincipleConfig config)
        {
            return AddNamedInstance<StorageQueueMessenger>(services, null, new StorageQueueMessenger(config));
        }


        private static IServiceCollection AddNamedInstance<T>(IServiceCollection services, string key, StorageQueueMessenger instance)
            where T : INamedInstance
        {
            if (!key.IsNullOrEmpty())
            {
                instance.Name = key;
            }

            services.AddSingleton(typeof(T), instance);

            // Ensure there's a NamedInstance factory to allow named collections of the messenger.
            services.AddFactoryIfNotAdded<T>();

            return services;
        }
    }
}
