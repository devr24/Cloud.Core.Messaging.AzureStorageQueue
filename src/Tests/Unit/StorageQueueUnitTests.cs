using System;
using System.Collections.Generic;
using Cloud.Core.Exceptions;
using Cloud.Core.Messaging.AzureStorageQueue.Config;
using Cloud.Core.Messaging.AzureStorageQueue.Models;
using Cloud.Core.Testing;
using FluentAssertions;
using Microsoft.Azure.Storage.Queue;
using Newtonsoft.Json;
using Xunit;

namespace Cloud.Core.Messaging.AzureStorageQueue.Tests.Unit
{
    [IsUnit]
    public class StorageQueueUnitTests
    {
        /// <summary>Verify message entity can be initialised as expected.</summary>
        [Fact]
        public void Test_MessageEntity_Initialise()
        {
            // Arrange/Act
            _ = new MessageEntity<string>("");
            _ = new MessageEntity<string> ("", null);
            var wrapper = new MessageEntity<string>() {Body = null, OriginalMessage = null, Properties = null};

            // Assert
            Assert.Null(wrapper.Body);
            Assert.Null(wrapper.OriginalMessage);
            Assert.Null(wrapper.Properties);
        }

        /// <summary>Verify message entity can be converted to and from json.</summary>
        [Fact]
        public void Test_MessageEntity_ToFromJson()
        {
            // Arrage 
            var wrapper = new MessageEntity<string>("body", new KeyValuePair<string, object>[1]
            {
                new KeyValuePair<string, object>("key", "value")
            });

            // Act
            var jsonVersion = wrapper.AsJson();
            var converted = JsonConvert.DeserializeObject<MessageEntity<string>>(jsonVersion);

            // Assert
            converted.Body.Should().Be("body");
            converted.Properties.Should().BeEquivalentTo(new KeyValuePair<string, object>[1]
            {
                new KeyValuePair<string, object>("key", "value")
            });
        }

        /// <summary>Verify message entity can be converted from json.</summary>
        [Fact]
        public void Test_MessageEntity_FromJson()
        {
            // Arrange/Act
            var json = "{\"Body\":\"body\",\"Properties\":{\"key\":\"value\"}}";
            var entity = MessageEntity<string>.FromJson(json);

            // Assert
            entity.Body.Should().Be("body");
        }
        
        /// <summary>Verify message entity can be converted using a cloud queue message.</summary>
        [Fact]
        public void Test_MessageEntity_FromCloudMessage()
        {
            // Arrange/Act
            var json = "{\"Body\":\"body\",\"Properties\":{\"key\":\"value\"}}";
            var entity = MessageEntity<string>.FromJson(new CloudQueueMessage(json));

            // Assert
            entity.Body.Should().Be("body");
        }

        /// <summary>Verify MsiConfig validation happens as expected.</summary>
        [Fact]
        public void Test_MsiConfig_Validate()
        {
            // Arrange
            var msiConfig = new MsiConfig()
            {
                InstanceName = null,
                SubscriptionId = null,
                TenantId = null,
                Receiver = null,
                Sender = null
            };

            // Act/Assert
            Assert.Throws<ValidateException>(() => msiConfig.ThrowIfInvalid());
            Assert.Null(msiConfig.InstanceName);
            Assert.Null(msiConfig.Receiver);
            Assert.Null(msiConfig.Sender);
            Assert.Null(msiConfig.SubscriptionId);
            Assert.Null(msiConfig.TenantId);
            msiConfig.ToString().Should().NotBeNullOrEmpty();
        }

        /// <summary>Verify Connection Config validation happens as expected.</summary>
        [Fact]
        public void Test_ConnectionConfig_Validate()
        {
            // Arrange
            var connectionConfig = new ConnectionConfig()
            {
                ConnectionString = null,
                Sender = null,
                Receiver = null
            };

            // Act/Assert
            Assert.Throws<ValidateException>(() => connectionConfig.ThrowIfInvalid());
            Assert.Null(connectionConfig.ConnectionString);
            Assert.Null(connectionConfig.Sender);
            Assert.Null(connectionConfig.Receiver);
            Assert.Null(connectionConfig.InstanceName);
            connectionConfig.ToString().Should().NotBeNullOrEmpty();
        }

        /// <summary>Verify service principle Config validation happens as expected.</summary>
        [Fact]
        public void Test_ServicePrincipleConfig_Validate()
        {
            // Arrange
            var spConfig = new ServicePrincipleConfig()
            {
                InstanceName = null,
                SubscriptionId = null,
                TenantId = null,
                AppId = null,
                AppSecret = null,
                Sender = null,
                Receiver = null
            };

            // Act/Assert
            Assert.Throws<ValidateException>(() => spConfig.ThrowIfInvalid());
            Assert.Null(spConfig.TenantId);
            Assert.Null(spConfig.SubscriptionId);
            Assert.Null(spConfig.AppId);
            Assert.Null(spConfig.AppSecret);
            Assert.Null(spConfig.Sender);
            Assert.Null(spConfig.Receiver);
            Assert.Null(spConfig.InstanceName);
            spConfig.ToString().Should().NotBeNullOrEmpty();
        }

        /// <summary>Verify receiver validation happens as expected.</summary>
        [Fact]
        public void Test_ReceiverSetup_Validate()
        {
            // Arrange
            var receiverSetup = new ReceiverSetup()
            {
                CreateEntityIfNotExists = false,
                EntityName = "",
                PollFrequencyInSeconds = 1,
                RemoveSerializationFailureMessages = false
            };

            // Act/Assert
            Assert.Throws<ValidateException>(() => receiverSetup.ThrowIfInvalid());
            Assert.False(receiverSetup.CreateEntityIfNotExists);
            Assert.True(receiverSetup.EntityName == "");
            Assert.Equal(1, receiverSetup.PollFrequencyInSeconds);
            Assert.False(receiverSetup.RemoveSerializationFailureMessages);
            receiverSetup.ToString().Should().NotBeNullOrEmpty();
        }

        /// <summary>Verify sender validation happens as expected.</summary>
        [Fact]
        public void Test_SenderSetup_Validate()
        {
            // Arrange
            var senderSetup = new SenderSetup()
            {
                CreateEntityIfNotExists = false,
                EntityName = ""
            };

            // Act/Assert
            Assert.Throws<ValidateException>(() => senderSetup.ThrowIfInvalid());
            Assert.False(senderSetup.CreateEntityIfNotExists);
            Assert.True(senderSetup.EntityName == "");
            Assert.True(senderSetup.MaxMessageSizeBytes > 0);
            Assert.True(senderSetup.MaxMessageSizeKb > 0);
            senderSetup.ToString().Should().NotBeNullOrEmpty();
        }
    }
}
