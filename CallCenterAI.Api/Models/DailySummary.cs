namespace CallCenterAI.Api.Models;

public class DailySummary
{
    public int Id { get; set; }
    public string EmployeeId { get; set; } = "";
    public string Summary { get; set; } = "";
    public DateTime Date { get; set; } = DateTime.UtcNow;
}