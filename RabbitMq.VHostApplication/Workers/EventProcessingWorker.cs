using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using RabbitMq.VHostApplication.Models;

namespace RabbitMq.VHostApplication.Workers;

public class EventProcessingWorker
{
    private const string InputQueue = "event_processing_queue";
    private const string OutputQueue = "event_dispatch_queue";

    public void Start()
    {
        var factory = new ConnectionFactory() { HostName = "localhost", UserName = "admin", Password = "admin" };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(queue: InputQueue, durable: false, exclusive: false, autoDelete: false, arguments: null);
        channel.QueueDeclare(queue: OutputQueue, durable: false, exclusive: false, autoDelete: false, arguments: null);

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var eventRequest = JsonSerializer.Deserialize<EventRequest>(message);

                eventRequest.Timestamp = DateTime.UtcNow;
                eventRequest.Status = "FINALIZADO";

                var newBody = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(eventRequest));
                channel.BasicPublish(exchange: "", routingKey: OutputQueue, basicProperties: null, body: newBody);

                channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
            }
        };

        channel.BasicConsume(queue: InputQueue, autoAck: false, consumer: consumer);

        Task.Delay(-1).Wait();
    }
}
