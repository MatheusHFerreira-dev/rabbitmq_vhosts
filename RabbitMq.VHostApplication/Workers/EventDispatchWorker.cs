using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using RabbitMq.VHostApplication.Models;

namespace RabbitMq.VHostApplication.Workers;

public class EventDispatchWorker
{
    private const string InputQueue = "event_dispatch_queue";

    public void Start()
    {
        var factory = new ConnectionFactory() { HostName = "localhost", UserName = "admin", Password = "admin" };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(queue: InputQueue, durable: false, exclusive: false, autoDelete: false, arguments: null);

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var eventRequest = JsonSerializer.Deserialize<EventRequest>(message);

                var appVhost = $"app_{eventRequest.ApplicationId}_{eventRequest.ApplicationName.Replace(" ", "_").ToLower()}";
                string appQueue = "event_app_notification";

                var vhostFactory = new ConnectionFactory()
                {
                    HostName = "localhost",
                    VirtualHost = appVhost,
                    UserName = "admin",
                    Password = "admin"
                };

                using var vhostConnection = vhostFactory.CreateConnection();
                using var vhostChannel = vhostConnection.CreateModel();

                vhostChannel.QueueDeclare(queue: appQueue, durable: false, exclusive: false, autoDelete: false, arguments: null);
                vhostChannel.BasicPublish(exchange: "", routingKey: appQueue, basicProperties: null, body: body);


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
