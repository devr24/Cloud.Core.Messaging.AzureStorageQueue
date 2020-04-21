using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Cloud.Core.Messaging.AzureStorageQueue.Config;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Azure.Storage.RetryPolicies;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;
using Microsoft.Rest.TransientFaultHandling;

namespace Cloud.Core.Messaging.AzureStorageQueue
{
    /// <summary>
    /// Base class for Azure specific implementation of cloud storage queue.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public abstract class StorageQueueBase : INamedInstance
    {
        /// <summary>
        /// Holds a list of cached connection strings.
        /// </summary>
        internal static readonly ConcurrentDictionary<string, string> ConnectionStrings = new ConcurrentDictionary<string, string>();
        /// <summary>
        /// The logger
        /// </summary>
        internal readonly ILogger Logger;
        /// <summary>
        /// The msi configuration
        /// </summary>
        internal readonly ConfigBase Config;
        /// <summary>
        /// The connection string
        /// </summary>
        internal string ConnectionString;

        private CloudQueue _receiverQueue;
        private CloudQueue _senderQueue;
        /// <summary>
        /// Managed Identity/User configuration (if used).
        /// </summary>
        private readonly MsiConfig _msiConfig;
        /// <summary>
        /// Service principle configuration (if used).
        /// </summary>
        private readonly ServicePrincipleConfig _spConfig;
        /// <summary>
        /// The cloud client
        /// </summary>
        private CloudQueueClient _cloudClient;
        /// <summary>
        /// The expiry time
        /// </summary>
        private DateTimeOffset? _expiryTime;
        /// <summary>
        /// The instance name
        /// </summary>
        private readonly string _instanceName;
        /// <summary>
        /// The subscription identifier
        /// </summary>
        private readonly string _subscriptionId;

        /// <summary>
        /// Gets the cloud table client.
        /// </summary>
        /// <value>The cloud table client.</value>
        internal CloudQueueClient CloudQueueClient
        {
            get
            {
                if (_cloudClient == null || _expiryTime <= DateTime.UtcNow)
                    InitializeClient();

                return _cloudClient;
            }
        }

        internal CloudQueue SenderQueue
        {
            get
            {
                if (Config.Sender == null)
                    return null;

                // Set the sender up for future use.
                if (_senderQueue == null)
                    _senderQueue = CloudQueueClient.GetQueueReference(Config.Sender.EntityName);

                return _senderQueue;
            }
        }

        internal CloudQueue ReceiverQueue
        {
            get
            {
                if (Config.Receiver == null)
                    return null;

                // Set the receiver up for future use.
                if (_receiverQueue == null)
                    _receiverQueue = CloudQueueClient.GetQueueReference(Config.Receiver.EntityName);

                return _receiverQueue;
            }
            set
            {
                _receiverQueue = value;
            }
        }

        /// <summary>Name of the object instance.</summary>
        public string Name { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StorageQueueBase"/> class.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="logger">The logger.</param>
        protected StorageQueueBase(ConnectionConfig config, ILogger logger = null)
        {
            // Ensure all mandatory fields are set.
            config.Validate();

            Logger = logger;
            ConnectionString = config.ConnectionString;
            Name = config.InstanceName;
            Config = config;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StorageQueueBase"/> class.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="logger">The logger.</param>
        protected StorageQueueBase(MsiConfig config, ILogger logger = null)
        {
            // Ensure all mandatory fields are set.
            config.Validate();

            Logger = logger;
            _msiConfig = config;
            Name = config.InstanceName;
            _instanceName = config.InstanceName;
            _subscriptionId = config.SubscriptionId;
            Config = config;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StorageQueueBase"/> class.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="logger">The logger.</param>
        protected StorageQueueBase(ServicePrincipleConfig config, ILogger logger = null)
        {
            // Ensure all mandatory fields are set.
            config.Validate();

            Logger = logger;
            _spConfig = config;
            Name = config.InstanceName;
            _instanceName = config.InstanceName;
            _subscriptionId = config.SubscriptionId;
            Config = config;
        }

        /// <summary>
        /// Initializes the client.
        /// </summary>
        /// <exception cref="InvalidOperationException">Cannot find storage account using connection string</exception>
        private void InitializeClient()
        {
            if (ConnectionString.IsNullOrEmpty())
                ConnectionString = BuildStorageConnection().GetAwaiter().GetResult();

            CloudStorageAccount.TryParse(ConnectionString, out var storageAccount);

            if (storageAccount == null)
                throw new InvalidOperationException("Cannot find storage account using connection string");

            // Create the CloudTableClient that represents the Table storage endpoint for the storage account.
            _cloudClient = storageAccount.CreateCloudQueueClient();
            CloudQueueClient.DefaultRequestOptions.RetryPolicy = new LinearRetry(TimeSpan.FromMilliseconds(500), 3);

            if (Config.Receiver != null && Config.Receiver.CreateEntityIfNotExists)
            {
                CloudQueueClient.GetQueueReference(Config.Receiver.EntityName)
                    .CreateIfNotExistsAsync().GetAwaiter().GetResult();
            }

            if (Config.Sender != null && Config.Sender.CreateEntityIfNotExists)
            {
                CloudQueueClient.GetQueueReference(Config.Sender.EntityName)
                    .CreateIfNotExistsAsync().GetAwaiter().GetResult();
            }
        }

        /// <summary>
        /// Builds a connection string for the storage account when none was specified during initialisation.
        /// </summary>
        /// <returns>Connection <see cref="string" /></returns>
        /// <exception cref="InvalidOperationException">If the Storage Namespace can not be resolved or access keys are not configured.</exception>
        internal async Task<string> BuildStorageConnection()
        {
            try
            {
                // If we already have the connection string for this instance - don't go get it again.
                if (ConnectionStrings.TryGetValue(_instanceName, out var connStr))
                {
                    return connStr;
                }

                const string azureManagementAuthority = "https://management.azure.com/";
                const string windowsLoginAuthority = "https://login.windows.net/";
                string token;

                // Use Msi Config if it's been specified, otherwise, use Service principle.
                if (_msiConfig != null)
                {
                    // Managed Service Identity (MSI) authentication.
                    var provider = new AzureServiceTokenProvider();
                    token = provider.GetAccessTokenAsync(azureManagementAuthority, _msiConfig.TenantId).GetAwaiter().GetResult();

                    if (string.IsNullOrEmpty(token))
                        throw new InvalidOperationException("Could not authenticate using Managed Service Identity, ensure the application is running in a secure context");

                    _expiryTime = DateTime.Now.AddDays(1);
                }
                else
                {
                    // Service Principle authentication
                    // Grab an authentication token from Azure.
                    var context = new AuthenticationContext($"{windowsLoginAuthority}{_spConfig.TenantId}");

                    var credential = new ClientCredential(_spConfig.AppId, _spConfig.AppSecret);
                    var tokenResult = context.AcquireTokenAsync(azureManagementAuthority, credential).GetAwaiter().GetResult();

                    if (tokenResult == null || tokenResult.AccessToken == null)
                        throw new InvalidOperationException($"Could not authenticate to {windowsLoginAuthority}{_spConfig.TenantId} using supplied AppId: {_spConfig.AppId}");

                    _expiryTime = tokenResult.ExpiresOn;
                    token = tokenResult.AccessToken;
                }

                // Set credentials and grab the authenticated REST client.
                var tokenCredentials = new TokenCredentials(token);

                var client = RestClient.Configure()
                    .WithEnvironment(AzureEnvironment.AzureGlobalCloud)
                    .WithLogLevel(HttpLoggingDelegatingHandler.Level.BodyAndHeaders)
                    .WithCredentials(new AzureCredentials(tokenCredentials, tokenCredentials, string.Empty, AzureEnvironment.AzureGlobalCloud))
                    .WithRetryPolicy(new RetryPolicy(new HttpStatusCodeErrorDetectionStrategy(), new FixedIntervalRetryStrategy(3, TimeSpan.FromMilliseconds(500))))
                    .Build();

                // Authenticate against the management layer.
                var azureManagement = Azure.Authenticate(client, string.Empty).WithSubscription(_subscriptionId);

                // Get the storage namespace for the passed in instance name.
                var storageNamespace = azureManagement.StorageAccounts.List().FirstOrDefault(n => n.Name == _instanceName);

                // If we cant find that name, throw an exception.
                if (storageNamespace == null)
                {
                    throw new InvalidOperationException($"Could not find the storage instance {_instanceName} in the subscription Id specified");
                }

                // Storage accounts use access keys - this will be used to build a connection string.
                var accessKeys = await storageNamespace.GetKeysAsync();

                // If the access keys are not found (not configured for some reason), throw an exception.
                if (accessKeys == null)
                {
                    throw new InvalidOperationException($"Could not find access keys for the storage instance {_instanceName}");
                }

                // We just default to the first key.
                var key = accessKeys[0].Value;

                // Build the connection string.
                var connectionString = $"DefaultEndpointsProtocol=https;AccountName={_instanceName};AccountKey={key};EndpointSuffix=core.windows.net";

                // Cache the connection string off so we don't have to reauthenticate.
                if (!ConnectionStrings.ContainsKey(_instanceName))
                {
                    ConnectionStrings.TryAdd(_instanceName, connectionString);
                }

                // Return connection string.
                return connectionString;
            }
            catch (Exception e)
            {
                _expiryTime = null;
                Logger?.LogError(e, "An exception occured during connection to Storage queue");
                throw new InvalidOperationException("An exception occurred during service connection, see inner exception for more detail", e);
            }
        }
    }
}
