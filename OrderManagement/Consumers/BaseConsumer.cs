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
        private readonly string _topicName;
        private readonly ConsumerConfig _consumerConfig;

        public BaseConsumer(ConsumerConfig config, string topicName)
        {
            _topicName = topicName;
            _consumerConfig = config;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(3000, stoppingToken);
            using var consumer = new ConsumerBuilder<string, string>(_consumerConfig).Build();
            var subscribed = false;

            while (!stoppingToken.IsCancellationRequested && !subscribed)
            {
                try
                {
                    consumer.Subscribe(_topicName);
                    subscribed = true;
                }
                catch (KafkaException ex)
                {
                    Console.WriteLine("Kafka not ready");
                    await Task.Delay(500, stoppingToken);
                }
            }

            await Task.Yield();

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = consumer.Consume(stoppingToken);
                    if (result.Message.Value == null) continue;

                    var ev = JsonSerializer.Deserialize<TEvent>(result.Message.Value);
                    if (ev != null)
                    {
                        await ProcessEventAsync(ev, stoppingToken);
                    }
                }
                catch (ConsumeException cEx)
                {
                    Console.WriteLine("Consumption error");
                    await Task.Delay(1000, stoppingToken);
                }
                catch (OperationCanceledException)
                {

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Unexpected error");
                    await Task.Delay(2000, stoppingToken);
                }
            }

            consumer.Close();
        }

        protected abstract Task ProcessEventAsync(TEvent @event, CancellationToken cancellationToken);
    }
}