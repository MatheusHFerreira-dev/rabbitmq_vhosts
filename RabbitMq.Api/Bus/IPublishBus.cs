using MassTransit;

namespace RabbitMq.Api.Bus;

public interface IPublishBus
{
    Task PublishAsync<T>(T message,CancellationToken ct = default) where T : class;
}


public class PublishBus : IPublishBus
{
    private readonly IBus _busPublisher;

    public PublishBus(IBus busPublisher)
    {
       _busPublisher = busPublisher;
    }

    public Task PublishAsync<T>(T message, CancellationToken ct = default) where T : class
    {
        return _busPublisher.Publish(message, ct);
    }
}