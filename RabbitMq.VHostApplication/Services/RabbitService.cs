using Microsoft.AspNetCore.Connections;
using RabbitMq.VHostApplication.Models;
using RabbitMQ.Client;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace RabbitMq.VHostApplication.Services;

public class RabbitService
{
    private readonly HttpClient _httpClient;
    private readonly string _rabbitMqApiUrl = "http://localhost:15672/api";
    private readonly string _adminUser = "admin";
    private readonly string _adminPassword = "admin";

    public RabbitService()
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
           Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_adminUser}:{_adminPassword}")));
    }

    public async Task<bool> CreateVHost(Application app)
    {
        var response = await _httpClient.PutAsync($"{_rabbitMqApiUrl}/vhosts/{app.VHost}", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> CreateUserForVHost(Application app)
    {
        var userPayload = new { password = app.Password, tags = "management" }; // Permissão para acessar o painel
        var jsonUser = JsonSerializer.Serialize(userPayload);
        var userContent = new StringContent(jsonUser, Encoding.UTF8, "application/json");

        var userResponse = await _httpClient.PutAsync($"{_rabbitMqApiUrl}/users/{app.Username}", userContent);
        if (!userResponse.IsSuccessStatusCode) return false;

        return await SetPermissions(app.VHost, app.Username);
    }

    private async Task<bool> SetPermissions(string vhost, string username)
    {
        var permissionPayload = new { configure = ".*", write = ".*", read = ".*" };
        var jsonPermissions = JsonSerializer.Serialize(permissionPayload);
        var permissionContent = new StringContent(jsonPermissions, Encoding.UTF8, "application/json");

        var permissionResponse = await _httpClient.PutAsync($"{_rabbitMqApiUrl}/permissions/{vhost}/{username}", permissionContent);
        return permissionResponse.IsSuccessStatusCode;
    }

    public bool SendMessageToQueue(string queueName, EventRequest eventRequest)
    {
        try
        {
            var factory = new ConnectionFactory() { HostName = "localhost", UserName = "admin", Password = "admin" };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(eventRequest));
            channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: null, body: body);

            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public List<string> GetMessagesFromAppVHost(Application app)
    {
        var messages = new List<string>();

        try
        {
            var factory = new ConnectionFactory()
            {
                HostName = "localhost",
                VirtualHost = app.VHost,
                UserName = "admin",
                Password = "admin"
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            string queueName = "event_app_notification";

            channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

            var result = channel.BasicGet(queueName, true);
            while (result != null)
            {
                var message = Encoding.UTF8.GetString(result.Body.ToArray());
                messages.Add(message);
                result = channel.BasicGet(queueName, true);
            }
        }
        catch (Exception ex)
        {
        }

        return messages;
    }

}


