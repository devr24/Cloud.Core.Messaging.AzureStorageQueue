using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cloud.Core.Messaging.AzureStorageQueue.Config;
using Cloud.Core.Messaging.AzureStorageQueue.Models;
using Cloud.Core.Testing;
using Cloud.Core.Testing.Lorem;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Cloud.Core.Messaging.AzureStorageQueue
{
    using Tests.Integration;

    [IsIntegration]
    public class MessengerSendReceiveTests
    {
        private bool _stopWait;
        private readonly IConfiguration _config;

        public MessengerSendReceiveTests()
        {
            // Get configuration and console logger.
            _config = new ConfigurationBuilder().AddJsonFile("appSettings.json").Build();
        }

        [Fact]
        public void QueueMessenger_CreateQueue()
        {
            var messenger = GetMessengerClient();
            var testQueue = "new-queue-test";

            messenger.EntityManager.CreateEntity(new StorageQueueEntityConfig { EntityName = testQueue }).GetAwaiter().GetResult();
            var createdQueues = messenger.CloudQueueClient.ListQueues().Select(s => s.Name).ToList();
            Assert.Contains(testQueue, createdQueues);
        }

        [Fact]
        public void QueueMessenger_Count()
        {
            // Arrange - Setup test data.
            var messenger = GetMessengerClient();
            var message = GetTestMessages(1).FirstOrDefault();

            var countBefore = messenger.EntityManager.GetSenderMessageCount().GetAwaiter().GetResult().ActiveEntityCount;

            messenger.Send(message).GetAwaiter().GetResult();

            var countAfter = messenger.EntityManager.GetSenderMessageCount().GetAwaiter().GetResult().ActiveEntityCount;

            Assert.True(countAfter == countBefore + 1);

            _ = messenger.EntityManager.GetReceiverMessageCount().GetAwaiter().GetResult().ActiveEntityCount;
        }

        /// <summary>Send a single message and ensure we can receive it again using the ReceiveOne method.</summary>
        [Fact]
        public void QueueMessenger_ReceiveOne()
        {
            // Arrange - Setup test data.
            var messenger = GetMessengerClient();
            var sendMsg = GetTestMessages(1).FirstOrDefault();

            // Act - send and then receive the sent message.
            messenger.Send(sendMsg).GetAwaiter().GetResult();

            var recMsg = messenger.ReceiveOne<ExampleModel>();

            var count = 0;
            while (recMsg == null && count < 10)
            {
                Thread.Sleep(TimeSpan.FromSeconds(20));
                recMsg = messenger.ReceiveOne<ExampleModel>();
                count++;
            }

            // Assert - message received should be the same as the one sent.
            recMsg.Should().BeEquivalentTo(sendMsg);
        }

        [Fact]
        public void QueueMessenger_EntityManager_GetReceiverEntityUsagePercentage()
        {
            // Arrange - Setup test data.
            var messenger = GetMessengerClient();

            Assert.ThrowsAsync<NotImplementedException>(async () => await messenger.EntityManager.GetReceiverEntityUsagePercentage());
        }

        [Fact]
        public void QueueMessenger_EntityManager_GetSenderEntityUsagePercentage()
        {
            // Arrange - Setup test data.
            var messenger = GetMessengerClient();

            Assert.ThrowsAsync<NotImplementedException>(async () => await messenger.EntityManager.GetSenderEntityUsagePercentage());
        }

        [Fact]
        public void QueueMessenger_EntityManager_IsReceiverEntityDisabled()
        {
            // Arrange - Setup test data.
            var messenger = GetMessengerClient();

            Assert.False(messenger.EntityManager.IsReceiverEntityDisabled().GetAwaiter().GetResult());
        }

        [Fact]
        public void QueueMessenger_EntityManager_IsSenderEntityDisabled()
        {
            // Arrange - Setup test data.
            var messenger = GetMessengerClient();

            Assert.False(messenger.EntityManager.IsSenderEntityDisabled().GetAwaiter().GetResult());
        }

        [Fact]
        public void QueueMessenger_EntityManager_DeleteEntity()
        {
            // Arrange - Setup test data.
            var messenger = GetMessengerClient();
            messenger.EntityManager.CreateEntity(new StorageQueueEntityConfig {EntityName = "somename" }).GetAwaiter().GetResult();
            Task.Delay(2000);
            AssertExtensions.DoesNotThrow(() => messenger.EntityManager.DeleteEntity("somename").GetAwaiter().GetResult());
        }

        /// <summary>Send a message to two queues and ensure we can receive both using the ReceiveOne method and switching between both again.</summary>
        [Fact]
        public void QueueMessenger_ReceiveOneUpdateReceiver()
        {
            // Arrange - Setup test data.
            var messengerOne = GetMessengerClient("UpdateQueueOne1");
            var messengerTwo = GetMessengerClient("UpdateQueueTwo2");

            var messageOne = new ExampleModel() { PropA = "Yeo A", PropB = 1, PropC = true };
            var messageTwo = new ExampleModel() { PropA = "Baa A", PropB = 2, PropC = false };

            // Act - send and then receive the sent message.
            messengerOne.Send(messageOne).GetAwaiter().GetResult();
            messengerTwo.Send(messageTwo).GetAwaiter().GetResult();

            // Get the first message
            var firstRetrievedMessage = messengerOne.ReceiveOne<ExampleModel>();
            messengerOne.Complete(firstRetrievedMessage).GetAwaiter().GetResult();
            // Update the receiver to queue 2
            messengerOne.UpdateReceiver("UpdateQueueTwo2").GetAwaiter().GetResult();

            // Get the second message
            var secondRetrievedMessage = messengerOne.ReceiveOne<ExampleModel>();
            messengerOne.Complete(secondRetrievedMessage).GetAwaiter().GetResult();
            // Assert - message received should be the same as the one sent.
            Assert.True(firstRetrievedMessage.PropA == "Yeo A");
            Assert.True(secondRetrievedMessage.PropA == "Baa A");
        }

        /// <summary>Send a message to two queues and ensure we can receive both using the ReceiveOne method and switching between both again.</summary>
        [Fact]
        public void QueueMessenger_ReceiveOneUpdateReceiverUninitReceiver()
        {
            // Arrange - Setup test data.
            var testUpdateReciever = new StorageQueueMessenger(new ConnectionConfig
            {
                ConnectionString = _config.GetValue<string>("ConnectionString")
            });

            var messengerOne = GetMessengerClient("UpdateQueueOneA");
            var messengerTwo = GetMessengerClient("UpdateQueueTwoB");

            var messageOne = new ExampleModel() { PropA = "Yeo A", PropB = 1, PropC = true };
            var messageTwo = new ExampleModel() { PropA = "Baa A", PropB = 2, PropC = false };

            // Act - send and then receive the sent message.
            messengerOne.Send(messageOne).GetAwaiter().GetResult();
            messengerTwo.Send(messageTwo).GetAwaiter().GetResult();

            // Get the first message
            var firstRetrievedMessage = messengerOne.ReceiveOne<ExampleModel>();
            messengerOne.Complete(firstRetrievedMessage).GetAwaiter().GetResult();
            // Update the receiver to queue 2
            testUpdateReciever.UpdateReceiver("UpdateQueueTwoB").GetAwaiter().GetResult();

            // Get the second message
            var secondRetrievedMessage = testUpdateReciever.ReceiveOne<ExampleModel>();
            testUpdateReciever.Complete(secondRetrievedMessage).GetAwaiter().GetResult();
            // Assert - message received should be the same as the one sent.
            Assert.True(firstRetrievedMessage.PropA == "Yeo A");
            Assert.True(secondRetrievedMessage.PropA == "Baa A");
        }

        /// <summary>Send a message to two queues and ensure we can receive both using the ReceiveOne method and switching between both again.</summary>
        [Fact]
        public void QueueMessenger_ReceiveOneShouldNotErrorWhenNoMessageIsAvailable()
        {
            // Arrange - Setup test data.
            _ = new StorageQueueMessenger(new ConnectionConfig
            {
                ConnectionString = _config.GetValue<string>("ConnectionString")
            });

            var messengerOne = GetMessengerClient("UpdateQueueOne10");

            var retrievedMessage = messengerOne.ReceiveOne<ExampleModel>();

            Assert.True(retrievedMessage == null);
        }

        /// <summary>Attempt to send a message thats larger than the allowed size.</summary>
        [Fact]
        public void QueueMessenger_LargeMessage()
        {
            // Arrange - Setup test data.
            var messenger = GetMessengerClient();
            var largeMessage = string.Join(" ", Lorem.GetParagraphs(50));

            var bytes = largeMessage.Length * sizeof(char) + sizeof(int);
            var kb = bytes / 1024;

            // Assert that the expected exception is thrown.
            Assert.Throws<ArgumentOutOfRangeException>(() => messenger.Send(largeMessage).GetAwaiter().GetResult());
        }

        /// <summary>Receive a message using callbacks and complete it.</summary>
        [Fact]
        public void QueueMessenger_ReceiveComplete()
        {
            // Arrange - Setup test data.
            var messenger = GetMessengerClient();
            var message = GetTestMessages(1).FirstOrDefault();
            messenger.Send(message).GetAwaiter().GetResult();

            // test that we receive this message...

            WaitTimeoutAction(() =>
            {
                messenger.Receive<ExampleModel>(msg =>
                {
                    messenger.Complete(msg).GetAwaiter().GetResult();
                }, err => { });
            }).GetAwaiter().GetResult();

            _stopWait = true;
            messenger.CancelReceive<ExampleModel>();
        }

        /// <summary>Receive a message using observables and complete it.</summary>
        [Fact]
        public void QueueMessenger_ReceiveObservableComplete()
        {
            // Arrange - Setup test data.
            var messenger = GetMessengerClient();
            var message = GetTestMessages(1).FirstOrDefault();

            // use observable batch to pull a group of messages
            WaitTimeoutAction(() =>
            {
                messenger.StartReceive<ExampleModel>().Subscribe(msg =>
                {
                    // Abandon the message - puts it back on the queue.
                    messenger.Abandon(msg).GetAwaiter().GetResult();
                });
            });
        }

        [Fact]
        public void QueueMessenger_ReceiveError()
        {
            // Arrange - Setup test data.
            var messenger = GetMessengerClient();
            var message = GetTestMessages(1).FirstOrDefault();

            // Error when message comes in that is not serialized
            WaitTimeoutAction(() =>
            {
                messenger.StartReceive<ExampleModel>().Subscribe(msg =>
                {
                    // Abandon the message - puts it back on the queue.
                    messenger.Error(msg).GetAwaiter().GetResult();
                });
            });

            messenger.CancelReceive<ExampleModel>();
            messenger.Dispose();
        }

        [Fact]
        public void QueueMessenger_ReceiveAbandon()
        {
            // Arrange - Setup test data.
            var messenger = GetMessengerClient();
            var message = GetTestMessages(1).FirstOrDefault();

            messenger.Send(message).GetAwaiter().GetResult();

            // count before

            // Take message, then put back on the queue.
            WaitTimeoutAction(() =>
            {
                // count during..

                messenger.StartReceive<ExampleModel>().Subscribe(msg =>
            {
                // Abandon the message - puts it back on the queue.
                messenger.Abandon(msg).GetAwaiter().GetResult();
            });
            });

            // count after

            messenger.CancelReceive<ExampleModel>();
            messenger.Dispose();
        }

        [Fact]
        public void QueueMessenger_SendOne()
        {
            // Arrange - Setup test data.
            var messenger = GetMessengerClient();
            var message = GetTestMessages(1).FirstOrDefault();

            // Count before

            messenger.Send(message).GetAwaiter().GetResult();

            // Do count before and after for test...
        }

        [Fact]
        public void QueueMessenger_SendBatch()
        {
            // Arrange - Setup test data.
            var messenger = GetMessengerClient();
            var messages = GetTestMessages();

            // Count before

            messenger.SendBatch(messages).GetAwaiter().GetResult();

            // Do count here....
        }

        /// <summary>Ensure that the messages sent as a batch, with a set groups properties, are correctly received.</summary>
        [Fact]
        public void QueueMessenger_SendBatchWithProperties()
        {
            // Arrange - Setup test data.
            var messenger = GetMessengerClient();
            var messages = GetTestMessages();
            var props = new[] {
                new KeyValuePair<string, object>("PropA", "ValueA"),
                new KeyValuePair<string, object>("PropB", "ValueB"),
                new KeyValuePair<string, object>("PropC", "ValueC"),
            };

            // Act - Send messages to the storage queue with props.
            messenger.SendBatch(messages, props).GetAwaiter().GetResult();

            WaitTimeoutAction(() =>
            {
                messenger.StartReceive<ExampleModel>().Subscribe(msg =>
                {
                    var readProps = messenger.ReadProperties(msg);
                    messenger.Complete(msg).GetAwaiter().GetResult();

                    // Assert - that we receive the properties for all the messages received.
                    readProps.Should().BeEquivalentTo(props);
                },
                err => { });
            }).GetAwaiter().GetResult();

            messenger.CancelReceive<ExampleModel>();
            messenger.Dispose();
        }

        /// <summary>Ensure that the messages sent as a batch, with a set groups properties, are correctly received.</summary>
        [Fact]
        public void QueueMessenger_SendBatchWithPropertyFunction()
        {
            // Arrange - Setup test data.
            var messenger = GetMessengerClient();
            var messages = GetTestMessages();

            // Act - Send messages to the storage queue with props, built using a function.
            messenger.SendBatch(messages, msg =>
                {
                    // Properties are built using function.
                    return new[]
                {
                        new KeyValuePair<string, object>("PropA", msg.PropA),
                        new KeyValuePair<string, object>("PropB", msg.PropB),
                        new KeyValuePair<string, object>("PropC", msg.PropC)
                    };
                }).GetAwaiter().GetResult();

            WaitTimeoutAction(() =>
            {
                messenger.Receive<ExampleModel>(msg =>
                {
                    var expectedProps = new[]{
                        new KeyValuePair<string, object>("PropA", msg.PropA),
                        new KeyValuePair<string, object>("PropB", msg.PropB),
                        new KeyValuePair<string, object>("PropC", msg.PropC)
                    };
                    var readProps = messenger.ReadProperties(msg);
                    messenger.Complete(msg).GetAwaiter().GetResult();

                    // Assert - that we receive the properties for all the messages received.
                    readProps.Should().BeEquivalentTo(expectedProps);
                },
                err => { }, 5);
            }).GetAwaiter().GetResult();

            messenger.CancelReceive<ExampleModel>();
            messenger.Dispose();
        }

        [Theory]
        [ClassData(typeof(SignedAccessUrlTestData))]
        public void Test_StorageQueue_GetSignedAccessUrl(Dictionary<string, string> expectedOutputs, ISignedAccessConfig testAccessConfig)
        {
            var testQueue = GetMessengerClient("testqueue");

            var accessUrl = testQueue.GetSignedAccessUrl(testAccessConfig);

            //Assertions
            Assert.NotNull(accessUrl);
            Assert.Contains(expectedOutputs["ExpiryDate"], accessUrl);
            Assert.Contains(expectedOutputs["Permission"], accessUrl);
            Assert.Contains(expectedOutputs["QueueName"], accessUrl);
        }

        [Fact]
        public void Test_StorageQueue_GetSignedAccessUrl_IncorrectPermissions()
        {
            var testQueue = GetMessengerClient("testqueue");
            var oneDayExpiry = DateTime.UtcNow.AddDays(1);

            var config = new SignedAccessConfig(new List<AccessPermission> { AccessPermission.List }, oneDayExpiry);

            var accessUrl = testQueue.GetSignedAccessUrl(config);

            //Assertions
            Assert.NotNull(accessUrl);
            Assert.Contains(oneDayExpiry.Date.ToString("yyyy-MM-dd"), accessUrl);
            Assert.DoesNotContain("sp=", accessUrl);
            Assert.Contains("testqueue", accessUrl);
        }

        private StorageQueueMessenger GetMessengerClient(string queueName = "testEntity")
        {
            return new StorageQueueMessenger(new ConnectionConfig
            {
                ConnectionString = _config.GetValue<string>("ConnectionString"),
                Receiver = new ReceiverConfig
                {
                    CreateEntityIfNotExists = true,
                    EntityName = queueName
                },
                Sender = new SenderConfig
                {
                    CreateEntityIfNotExists = true,
                    EntityName = queueName
                }
            });
        }

        private List<ExampleModel> GetTestMessages(int batchSize = 12)
        {
            var items = new List<ExampleModel>();

            for (int i = 0; i < batchSize; i++)
            {
                items.Add(new ExampleModel { PropA = $"Test Message {i}", PropB = i, PropC = i % 2 == 0 });
            }

            return items;
        }

        private Task WaitTimeoutAction(Action action)
        {
            _stopWait = false;
            int count = 0;

            action();

            do
            {
                Thread.Sleep(1000);
                count++;
            } while (!_stopWait && count < 20);

            return Task.FromResult(true);
        }

        private class ExampleModel
        {
            public string PropA { get; set; }
            public int PropB { get; set; }
            public bool PropC { get; set; }
        }
    }
}
