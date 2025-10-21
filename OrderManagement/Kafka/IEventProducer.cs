using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderManagement.Kafka
{
    public interface IEventProducer
    {
        Task ProduceAsync<T>(string topic, T @event, CancellationToken cancellationToken = default);
    }
}