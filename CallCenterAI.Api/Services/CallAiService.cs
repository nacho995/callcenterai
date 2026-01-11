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
        
        CallSummaryResponse? result;
        try
        {
            result = System.Text.Json.JsonSerializer.Deserialize<CallSummaryResponse>(jsonText);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR parsing AI response: {ex.Message}");
            // Fallback si el JSON no es válido
            result = new CallSummaryResponse
            {
                Category = "Conversación General",
                AirportCode = "MAD",
                Summary = transcript.Length > 100 ? transcript.Substring(0, 100) : transcript
            };
        }
        
        // Validar y limpiar campos vacíos
        if (string.IsNullOrWhiteSpace(result.AirportCode) || result.AirportCode == "UNKNOWN")
        {
            Console.WriteLine("No airport detected, using MAD as default");
            result.AirportCode = "MAD";
        }
        
        if (string.IsNullOrWhiteSpace(result.Category))
        {
            Console.WriteLine("No category detected, using default");
            result.Category = "Conversación General";
        }
        
        if (string.IsNullOrWhiteSpace(result.Summary))
        {
            Console.WriteLine("No summary detected, using transcript");
            result.Summary = transcript.Length > 200 ? transcript.Substring(0, 200) + "..." : transcript;
        }
        
        Console.WriteLine($"Final analysis - Airport: {result.AirportCode}, Category: {result.Category}");
        return result;
    }
}
