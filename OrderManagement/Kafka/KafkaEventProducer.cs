using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Confluent.Kafka;

namespace OrderManagement.Kafka
{
    public class KafkaEventProducer : IEventProducer, IDisposable
    {
        private readonly IProducer<string, string> _producer;

        public KafkaEventProducer(IConfiguration configuration)
        {
            var config = new ProducerConfig
            {
                BootstrapServers = configuration["Kafka:BootstrapServers"],
                Acks = Acks.All,
                EnableIdempotence = true,
                LingerMs = 5,
                MessageTimeoutMs = 30000
            };

            _producer = new ProducerBuilder<string, string>(config).Build();
        }

        public async Task ProduceAsync<T>(string topic, T @event, CancellationToken cancellationToken = default)
        {
            var message = new Message<string, string>
            {
                Key = Guid.NewGuid().ToString(),
                Value = System.Text.Json.JsonSerializer.Serialize(@event)
            };

            await _producer.ProduceAsync(topic, message, cancellationToken);
        }

        public void Dispose()
        {
            _producer.Flush(TimeSpan.FromSeconds(10));
            _producer.Dispose();
        }
    }
}