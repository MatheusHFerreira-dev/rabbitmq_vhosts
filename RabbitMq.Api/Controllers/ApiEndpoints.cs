using MassTransit;
using RabbitMq.Api.Bus;
using RabbitMq.Api.Relatorios;

namespace RabbitMq.Api.Controllers;

public static class ApiEndpoints
{
    //Forma interessante de criar as rotas da api
    public static void AddApiEndpoints(this WebApplication app)
    {
        //IBus, pra usar as filas, quando for usar, melhor encapsular em outro lugar e n usar diretamente
        app.MapPost("solicitar-relatorio/{name}", async (string name, IPublishBus bus, CancellationToken ct = default) =>
           {
               var solicitacao = new SolicitacaoRelatorio()
               {
                   Id = Guid.NewGuid(),
                   Nome = name,
                   Status = "Pendente",
                   ProcessedTime = null
               };

               Lista.Relatorios.Add(solicitacao);

               var eventResquest = new RelatorioSolicitadoEvent(solicitacao.Id, solicitacao.Nome);

               await bus.PublishAsync(eventResquest,ct);

               return Results.Ok(solicitacao);
           });

        app.MapGet("relatorios", () => Lista.Relatorios);
    }
}
