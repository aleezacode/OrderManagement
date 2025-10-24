using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Confluent.Kafka;
using MediatR;
using System.Text.Json;
using OrderManagement.Models.Events;

namespace OrderManagement.Consumers
{
    public abstract class BaseConsumer<TEvent> : BackgroundService where TEvent : class, IEvent
    {
        private readonly IConsumer<string, string> _consumer;
        private readonly string _topicName;

        public BaseConsumer(ConsumerConfig config, string topicName)
        {
            _topicName = topicName;

            var builder = new ConsumerBuilder<string, string>(config);
            _consumer = builder.Build();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _consumer.Subscribe(_topicName);

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var result = _consumer.Consume(stoppingToken);
                    if (result?.Message != null)
                    {
                        try
                        {
                            var message = JsonSerializer.Deserialize<TEvent>(result.Message.Value);
                            if (message != null)
                            {
                                await ProcessEventAsync(message, stoppingToken);
                                _consumer.Commit(result);
                            }
                        }
                        catch (System.Exception)
                        {
                            throw;
                        }
                    }
                }
            }
            finally
            {
                _consumer.Close();
            }
        }

        protected abstract Task ProcessEventAsync(TEvent @event, CancellationToken cancellationToken);

        public override void Dispose()
        {
            _consumer?.Dispose();
            base.Dispose();
        }
    }
}