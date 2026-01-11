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
        var prompt = $@"Analiza esta transcripción de una llamada y extrae información en formato JSON:

INSTRUCCIONES:
1. Categoría: clasifica el tema de la conversación (Información, Consulta, Queja, Saludo, Charla, Otros, etc.)
2. Aeropuerto: si se menciona un aeropuerto español, devuelve su código IATA (MAD, BCN, AGP, etc.)
   - Si NO se menciona ningún aeropuerto, usa ""MAD"" por defecto
3. Resumen: descripción breve de qué trata la conversación

CÓDIGOS DE AEROPUERTOS (solo si se mencionan):
MAD=Madrid, BCN=Barcelona, AGP=Málaga, VLC=Valencia, SVQ=Sevilla, ALC=Alicante, 
BIO=Bilbao, PMI=Palma, IBZ=Ibiza, LPA=Gran Canaria, TFS=Tenerife

Responde ÚNICAMENTE con JSON (sin ```json, sin explicaciones):
{{
  ""category"": ""<categoría>"",
  ""airportCode"": ""<código o MAD>"",
  ""summary"": ""<resumen>""
}}

TRANSCRIPCIÓN:
""""""{transcript}""""""
";

        var messages = new List<ChatMessage>
        {
            ChatMessage.CreateSystemMessage("Eres un asistente de call center experto. Analiza llamadas y extrae información estructurada. Responde SOLO con JSON válido, sin formato markdown."),
            ChatMessage.CreateUserMessage(prompt)
        };

        var response = await _client.CompleteChatAsync(messages);
        var jsonText = response.Value.Content[0].Text.Trim();
        
        // Limpiar markdown si viene con ```json
        if (jsonText.StartsWith("```"))
        {
            var lines = jsonText.Split('\n');
            jsonText = string.Join('\n', lines.Skip(1).Take(lines.Length - 2));
        }

        Console.WriteLine($"AI Response: {jsonText}");
        var result = System.Text.Json.JsonSerializer.Deserialize<CallSummaryResponse>(jsonText)!;
        
        // Si no se detectó aeropuerto, usar MAD por defecto
        if (string.IsNullOrEmpty(result.AirportCode) || result.AirportCode == "UNKNOWN")
        {
            Console.WriteLine("No airport detected, using MAD as default");
            result.AirportCode = "MAD";
        }
        
        return result;
    }
}
