namespace CallCenterAI.Api.Dtos;

public class CreateCallRequest
{
    public string? Transcript { get; set; }
    public string? FromNumber { get; set; }
}