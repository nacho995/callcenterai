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
            ChatMessage.CreateSystemMessage("Eres un experto en análisis de llamadas. Extrae información clave y genera resúmenes concisos. Responde ÚNICAMENTE con JSON sin formato markdown ni explicaciones adicionales."),
            ChatMessage.CreateUserMessage(prompt)
        };

        var chatOptions = new ChatCompletionOptions
        {
            Temperature = 0.3f, // Más determinístico
            MaxOutputTokenCount = 300
        };

        var response = await _client.CompleteChatAsync(messages, chatOptions);
        var jsonText = response.Value.Content[0].Text.Trim();
        
        // Limpiar markdown si viene con ```json
        if (jsonText.StartsWith("```"))
        {
            var lines = jsonText.Split('\n');
            jsonText = string.Join('\n', lines.Skip(1).Take(lines.Length - 2));
        }

        Console.WriteLine($"=== AI RAW RESPONSE ===");
        Console.WriteLine(jsonText);
        Console.WriteLine($"======================");
        
        CallSummaryResponse? result;
        try
        {
            result = System.Text.Json.JsonSerializer.Deserialize<CallSummaryResponse>(jsonText);
            Console.WriteLine($"✅ JSON parsed successfully");
            Console.WriteLine($"   Category from AI: '{result?.Category}'");
            Console.WriteLine($"   AirportCode from AI: '{result?.AirportCode}'");
            Console.WriteLine($"   Summary from AI: '{result?.Summary}'");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ ERROR parsing AI response: {ex.Message}");
            Console.WriteLine($"   Problematic JSON: {jsonText}");
            // Fallback si el JSON no es válido
            result = new CallSummaryResponse
            {
                Category = "Error de Análisis",
                AirportCode = "MAD",
                Summary = "No se pudo analizar correctamente"
            };
        }
        
        // Validar y limpiar campos vacíos
        if (string.IsNullOrWhiteSpace(result.AirportCode) || result.AirportCode == "UNKNOWN")
        {
            Console.WriteLine("⚠️  No airport detected by AI, using MAD as default");
            result.AirportCode = "MAD";
        }
        else
        {
            Console.WriteLine($"✅ Airport detected by AI: {result.AirportCode}");
        }
        
        if (string.IsNullOrWhiteSpace(result.Category))
        {
            Console.WriteLine("⚠️  No category detected by AI, using default");
            result.Category = "Conversación General";
        }
        else
        {
            Console.WriteLine($"✅ Category detected by AI: {result.Category}");
        }
        
        if (string.IsNullOrWhiteSpace(result.Summary))
        {
            Console.WriteLine("⚠️  No summary detected by AI, generating from transcript");
            result.Summary = $"Llamada sobre: {transcript.Substring(0, Math.Min(100, transcript.Length))}";
        }
        else
        {
            Console.WriteLine($"✅ Summary detected by AI: {result.Summary.Substring(0, Math.Min(50, result.Summary.Length))}...");
        }
        
        Console.WriteLine($"=== FINAL RESULT ===");
        Console.WriteLine($"Airport: {result.AirportCode}, Category: {result.Category}, Summary length: {result.Summary.Length}");
        return result;
    }
}
