
using RabbitMq.VHostApplication.Models;
using RabbitMq.VHostApplication.Response;
using RabbitMq.VHostApplication.Services;
using RabbitMq.VHostApplication.Workers;

namespace RabbitMq.VHostApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddSingleton<RabbitService>();
            builder.Services.AddSingleton<WorkerManager>();


            builder.Services.AddAuthorization();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            var applications = new List<Application>();
            var appCounter = 1;

            app.MapGet("/start-workers", (WorkerManager workerManager) =>
            {
                workerManager.StartWorkers();
                return Results.Ok("Workers iniciados!");
            });

            app.MapGet("/events/{applicationId}", (int applicationId, RabbitService rabbitMQ) =>
            {
                var appInstance = applications.FirstOrDefault(a => a.Id == applicationId);
                if (appInstance == null)
                    return Results.NotFound("Aplicação não encontrada!");

                var messages = rabbitMQ.GetMessagesFromAppVHost(appInstance);
                return Results.Ok(messages);
            });

            app.MapPost("/applications", async (string appName, RabbitService rabbitMQ) =>
            {
                if (string.IsNullOrEmpty(appName))
                    return Results.BadRequest("Nome da aplicação não pode ser vazio!");

                var appInstance = new Application
                {
                    Id = appCounter++,
                    Name = appName,
                    Password = "12345"
                };

                bool vhostCreated = await rabbitMQ.CreateVHost(appInstance);
                if (!vhostCreated)
                    return Results.Problem("Erro ao criar VHost no RabbitMQ.");

                bool userCreated = await rabbitMQ.CreateUserForVHost(appInstance);
                if (!userCreated)
                    return Results.Problem("Erro ao criar usuário para o VHost.");

                applications.Add(appInstance);

                return Results.Ok(new
                {
                    appInstance.Id,
                    appInstance.Name,
                    appInstance.VHost,
                    Username = appInstance.Username,
                    Password = appInstance.Password
                });
            });

            app.MapPost("/events", async (EventResponse request, RabbitService rabbitMQ) =>
            {
                var appInstance = applications.FirstOrDefault(a => a.Id == request.applicationId);
                if (appInstance == null)
                    return Results.NotFound("Aplicação não encontrada!");

                var completeEvent = new EventRequest()
                {
                    ApplicationId = request.applicationId,
                    ApplicationName = appInstance.Name,
                    Message = request.message                    
                };

                bool sent = rabbitMQ.SendMessageToQueue("event_creation_queue", completeEvent);
                if (!sent)
                    return Results.Problem("Erro ao enviar evento para a fila inicial.");

                return Results.Ok(new { status = "Evento enviado para processamento", queue = "event_creation_queue" });
            });     

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

             app.Run();
        }
    }
}
