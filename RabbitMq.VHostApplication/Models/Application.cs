namespace RabbitMq.VHostApplication.Models;

public class Application
{
    public int Id { get; set; }  
    public string Name { get; set; }  
    public string VHost => $"app_{Id}_{Name.Replace(" ", "_").ToLower()}"; 
    public string Username => $"user_{Id}_{Name.Replace(" ", "_").ToLower()}";  
    public string Password { get; set; }
}
