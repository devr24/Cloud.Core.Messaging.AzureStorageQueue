using System;
using Cloud.Core.Messaging.AzureStorageQueue.Config;
using Cloud.Core.Messaging.AzureStorageQueue.Models;
using Cloud.Core.Testing;
using Microsoft.Azure.Storage.Queue;
using Xunit;

namespace Cloud.Core.Messaging.AzureStorageQueue.Tests.Unit
{
    [IsUnit]
    public class PocoTests
    {
        [Fact]
        public void POCO_Tests()
        {
            _ = new MessageWrapper<string>("");
            _ = new MessageWrapper<string>("", null);

            var wrapper = new MessageWrapper<string>()
            {
                Body = null,
                OriginalMessage = null,
                Properties = null
            };

            Assert.Null(wrapper.Body);
            Assert.Null(wrapper.OriginalMessage);
            Assert.Null(wrapper.Properties);

            var x = wrapper.AsJson();
            try
            {
                var y = MessageWrapper<string>.FromJson(new CloudQueueMessage("Yeo"));
            }
            catch (Exception)
            {

            }

            var msiConfig = new MsiConfig()
            {
                InstanceName = null,
                Receiver = null,
                Sender = null,
                SubscriptionId = null,
                TenantId = null
            };

            Assert.Throws<ArgumentException>(() => msiConfig.Validate());

            Assert.Null(msiConfig.InstanceName);
            Assert.Null(msiConfig.Receiver);
            Assert.Null(msiConfig.Sender);
            Assert.Null(msiConfig.SubscriptionId);
            Assert.Null(msiConfig.TenantId);

            var connectionConfig = new ConnectionConfig()
            {
                ConnectionString = null,
                Sender = null,
                Receiver = null
            };

            Assert.Throws<ArgumentException>(() => connectionConfig.Validate());

            Assert.Null(connectionConfig.ConnectionString);
            Assert.Null(connectionConfig.Sender);
            Assert.Null(connectionConfig.Receiver);
            Assert.Null(connectionConfig.InstanceName);

            var receiverSetup = new ReceiverSetup()
            {
                CreateEntityIfNotExists = false,
                EntityName = "",
                PollFrequencyInSeconds = 1,
                RemoveSerializationFailureMessages = false
            };

            Assert.Throws<ArgumentException>(() => receiverSetup.Validate());

            Assert.False(receiverSetup.CreateEntityIfNotExists);
            Assert.True(receiverSetup.EntityName == "");
            Assert.Equal(1, receiverSetup.PollFrequencyInSeconds);
            Assert.False(receiverSetup.RemoveSerializationFailureMessages);

            _ = receiverSetup.ToString();

            var senderSetup = new SenderSetup()
            {
                CreateEntityIfNotExists = false,
                EntityName = ""
            };

            _ = senderSetup.ToString();

            Assert.Throws<ArgumentException>(() => senderSetup.Validate());

            Assert.False(senderSetup.CreateEntityIfNotExists);
            Assert.True(senderSetup.EntityName == "");
            Assert.True(senderSetup.MaxMessageSizeBytes > 0);
            Assert.True(senderSetup.MaxMessageSizeKb > 0);
        }
    }
}
