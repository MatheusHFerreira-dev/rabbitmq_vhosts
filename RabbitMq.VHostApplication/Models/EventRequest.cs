namespace RabbitMq.VHostApplication.Models;
public class EventRequest
{
    public int ApplicationId { get; set; } 
    public string ApplicationName { get; set; }
    public string Message { get; set; }  
    public string Status { get; set; } = "NOVO"; 
    public DateTime? Timestamp { get; set; }

    public override string ToString()
    {
        return $"[Evento] App: {ApplicationName} | Status: {Status} | Mensagem: {Message} | Timestamp: {Timestamp?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A"}";
    }
}

