using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;
using System;

namespace DotnetCurd.Repository
{
    public class MessagePublisher :IMessagePublisher
    {
        private readonly ITopicClient topicClient;
        public MessagePublisher(ITopicClient topicClient)
        {
            this.topicClient = topicClient;
        }
        public async Task PublisherAsync<T>(T request)
        {
            var message = new Message
            {
                MessageId = Guid.NewGuid().ToString(),
                Body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(request)),
                SessionId = Guid.NewGuid().ToString()
                

        };
            message.UserProperties.Add("MessageType", typeof(T).Name);
            await topicClient.SendAsync(message);
        }
    }
}
