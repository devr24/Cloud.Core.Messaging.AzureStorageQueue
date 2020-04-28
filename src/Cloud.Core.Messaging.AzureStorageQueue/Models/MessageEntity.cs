using System.Collections.Generic;
using Microsoft.Azure.Storage.Queue;
using Newtonsoft.Json;

namespace Cloud.Core.Messaging.AzureStorageQueue.Models
{
    internal class MessageEntity<T> : IMessageEntity<T>
            where T : class
    {
        public MessageEntity() { }

        public MessageEntity(T body)
        {
            Body = body;
        }

        public MessageEntity(T body, KeyValuePair<string, object>[] props)
        {
            Body = body;
            if (!props.IsNullOrDefault())
            {
                Properties = new Dictionary<string, object>(props);
            }
        }

        public T Body { get; set; }
        public IDictionary<string, object> Properties { get; set; }

        public string AsJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public O GetPropertiesTyped<O>() where O : class, new()
        {
            return Properties.ToObject<O>();
        }

        public static MessageEntity<T> FromJson(CloudQueueMessage msg)
        {
            var converted = JsonConvert.DeserializeObject<MessageEntity<T>>(msg.AsString);
            return converted;
        }

        public static MessageEntity<T> FromJson(string json)
        {
            var converted = JsonConvert.DeserializeObject<MessageEntity<T>>(json);
            return converted;
        }

        public bool IsEmpty()
        {
            if (Body == null && Properties == null)
            {
                return true;
            }

            return false;
        }

        [JsonIgnore]
        public CloudQueueMessage OriginalMessage { get; set; }
    }
}
