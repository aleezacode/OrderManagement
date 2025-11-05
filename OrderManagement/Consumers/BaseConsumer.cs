using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Confluent.Kafka;
using MediatR;
using System.Text.Json;
using OrderManagement.Models.Events;
using Microsoft.Extensions.Logging; 

namespace OrderManagement.Consumers
{
    public abstract class BaseConsumer<TEvent> : BackgroundService where TEvent : class, IEvent
    {
        private readonly string _topicName;
        private readonly ConsumerConfig _consumerConfig;
        private readonly ILogger _logger;

        public BaseConsumer(ConsumerConfig config, string topicName, ILogger logger)
        {
            _topicName = topicName;
            _consumerConfig = config;
            _logger = logger;
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
                    _logger.LogInformation($"Successfully subscribed to topic: {_topicName}");
                }
                catch (KafkaException ex)
                {
                    _logger.LogWarning(ex, $"Kafka not ready. Retrying subscription to topic: {_topicName}");
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
                        consumer.Commit(result);

                        try
                        {
                            await ProcessEventAsync(ev, stoppingToken);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Error processing event of type: {typeof(TEvent).Name}");
                        }
                    }
                }
                catch (ConsumeException cEx)
                {
                    _logger.LogError(cEx, $"Consumption error for topic: {_topicName}");
                    await Task.Delay(1000, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation($"Consumer operation cancelled for topic: {_topicName}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Unexpected error for consumer topic {_topicName}");
                    await Task.Delay(2000, stoppingToken);
                }
            }

            consumer.Close();
            _logger.LogInformation($"Consumer closed for topic: {_topicName}");
        }

        protected abstract Task ProcessEventAsync(TEvent @event, CancellationToken cancellationToken);
    }
}