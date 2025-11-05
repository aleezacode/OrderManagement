using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Confluent.Kafka;
using OrderManagement.Repositories;
using OrderManagement.Models;

namespace OrderManagement.Kafka
{
    public class KafkaEventProducer : IEventProducer, IDisposable
    {
        private readonly IRepository<EventPublishlog> _eventPublishlogRepository;
        private readonly IProducer<string, string> _producer;

        public KafkaEventProducer(IConfiguration configuration, IRepository<EventPublishlog> eventPublishlogRepository)
        {
            _eventPublishlogRepository = eventPublishlogRepository;

            var config = new ProducerConfig
            {
                BootstrapServers = configuration["Kafka:BootstrapServers"],
                Acks = Acks.All,
                EnableIdempotence = false,
                LingerMs = 5,
                MessageTimeoutMs = 30000
            };

            _producer = new ProducerBuilder<string, string>(config).Build();
        }

        public async Task ProduceAsync<T>(string topic, T @event, CancellationToken cancellationToken = default)
        {
            var message = new Message<string, string>
            {
                Key = @event!.GetType().Name,
                Value = System.Text.Json.JsonSerializer.Serialize(@event)
            };

            await _producer.ProduceAsync(topic, message, cancellationToken);

            var eventPublishlog = new EventPublishlog()
            {
                OrderId = GetOrderId(@event),
                EventType = @event!.GetType().Name,
                EventMessage = message.Value,
                PublishedAt = DateTime.UtcNow
            };

            await _eventPublishlogRepository.CreateAsync(eventPublishlog);
        }

        private string? GetOrderId(object @event)
        {
            try
            {
                return (@event as dynamic)?.OrderId;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void Dispose()
        {
            _producer.Flush(TimeSpan.FromSeconds(10));
            _producer.Dispose();
        }
    }
}