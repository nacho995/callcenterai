namespace CallCenterAI.Api.Models;

public class Call
{
    public int Id { get; set; }
    public string EmployeeId { get; set; } = "";
    public int AirportId { get; set; }
    public Airport? Airport { get; set; }
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
    public string Transcript { get; set; } = "";
    public string Summary { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}