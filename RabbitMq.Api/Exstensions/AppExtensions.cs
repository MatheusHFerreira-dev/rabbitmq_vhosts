using MassTransit;
using RabbitMq.Api.Bus;
using System.Runtime.CompilerServices;

namespace RabbitMq.Api.Exstensions;

public static class AppExtensions
{
    public static void AddRabbitMqService(this IServiceCollection services)
    {
        services.AddTransient<IPublishBus, PublishBus>();
        services.AddMassTransit(busConfig => 
        {
            //Adicionando consumer ao rabbitMq
            busConfig.AddConsumer<RelatorioSolicitadoEventConsumer>();

            busConfig.UsingRabbitMq((ctx,cfg) =>
            {

                cfg.Host(new Uri("amqp://localhost:5672"),host =>
                {
                    host.Username("guest");
                    host.Password("guest");
                });

                cfg.ConfigureEndpoints(ctx);
            });        
        });
    }
}
