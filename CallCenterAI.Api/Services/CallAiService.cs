using CallCenterAI.Api.Dtos;
using OpenAI.Chat;

namespace CallCenterAI.Api.Services;

public class CallAiService
{
    private readonly ChatClient _client;
    private readonly string _model;

    public CallAiService(IConfiguration config)
    {
        _model = config["OpenAI:Model"]!;
        _client = new ChatClient(_model, config["OpenAI:ApiKey"]!);
    }

    public async Task<CallSummaryResponse> AnalyzeAsync(string transcript)
    {
        var prompt = $@"Analiza esta llamada y devuelve este JSON exacto:

{{
  ""category"": ""<categoría de la llamada>"",
  ""airportCode"": ""<código IATA del aeropuerto: MAD, BCN, AGP, etc>"",
  ""summary"": ""<resumen breve de la llamada>""
}}

Llamada:
""""""{transcript}""""""
";

        var messages = new List<ChatMessage>
        {
            ChatMessage.CreateSystemMessage("Eres un asistente de call center inteligente. Responde SOLO en JSON."),
            ChatMessage.CreateUserMessage(prompt)
        };

        var response = await _client.CompleteChatAsync(messages);
        var json = response.Value.Content[0].Text;

        return System.Text.Json.JsonSerializer.Deserialize<CallSummaryResponse>(json!)!;
    }
}
