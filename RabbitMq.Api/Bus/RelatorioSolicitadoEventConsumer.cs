using MassTransit;
using RabbitMq.Api.Relatorios;

namespace RabbitMq.Api.Bus;

public class RelatorioSolicitadoEventConsumer : IConsumer<RelatorioSolicitadoEvent>
{
    private readonly ILogger<RelatorioSolicitadoEventConsumer> _logger;

    public RelatorioSolicitadoEventConsumer(ILogger<RelatorioSolicitadoEventConsumer> logger)
    {
        _logger = logger;
    }
    public async Task Consume(ConsumeContext<RelatorioSolicitadoEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation("Processando relatório Id:{Id}, Nome:{Nome}", message.Id, message.Name);

        //Delay falso
        await Task.Delay(10000);

        //Atualizando o status
        var relatorio = Lista.Relatorios.FirstOrDefault(x => x.Id == message.Id);

        if (relatorio != null)
        {
            relatorio.Status = "Processado";
            relatorio.ProcessedTime = DateTime.Now;
        }
        _logger.LogInformation("Relatório processado Id:{Id}, Nome:{Nome}", message.Id, message.Name);

    }
}
