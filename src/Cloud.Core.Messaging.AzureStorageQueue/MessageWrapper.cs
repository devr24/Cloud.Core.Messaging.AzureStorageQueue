using System.Collections.Generic;
using Microsoft.Azure.Storage.Queue;
using Newtonsoft.Json;

namespace Cloud.Core.Messaging.AzureStorageQueue
{
    internal class MessageWrapper<T> : IMessageEntity<T>
            where T : class
    {
        public MessageWrapper() { }

        public MessageWrapper(T body)
        {
            Body = body;
        }

        public MessageWrapper(T body, KeyValuePair<string, object>[] props)
        {
            Body = body;
            Properties = props?.AsDictionary();
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

        public static MessageWrapper<T> FromJson(CloudQueueMessage msg)
        {
            var converted = JsonConvert.DeserializeObject(msg.AsString) as MessageWrapper<T>;
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
