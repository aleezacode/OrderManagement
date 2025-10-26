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

            try
            {
                _consumer.Subscribe(_topicName);
            }
            catch (System.Exception)
            {
                return;
            }

            while (!stoppingToken.IsCancellationRequested)
            {
        try
        {
            var result = _consumer.Consume(stoppingToken);
            if (result?.Message?.Value == null) continue;

            var ev = JsonSerializer.Deserialize<TEvent>(result.Message.Value);
            if (ev != null)
                await ProcessEventAsync(ev, stoppingToken);
        }
        catch (ConsumeException ce)
        {
            await Task.Delay(1000, stoppingToken);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            await Task.Delay(1000, stoppingToken);
        }
            }

            _consumer.Close();
        }

        protected abstract Task ProcessEventAsync(TEvent @event, CancellationToken cancellationToken);

        public override void Dispose()
        {
            _consumer?.Dispose();
            base.Dispose();
        }
    }
}