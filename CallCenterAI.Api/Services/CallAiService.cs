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
        var prompt = $@"Eres un asistente de IA que analiza llamadas de call center de aeropuertos.

TAREA: Analiza la transcripción y extrae:

1. CATEGORÍA (elige UNA que mejor describa el tema principal):
   - Parking (si pregunta por aparcamiento, parking, estacionamiento)
   - Vuelos (si pregunta por horarios, salidas, llegadas, retrasos de vuelos)
   - Facturación (si pregunta por check-in, documentación, equipaje facturado)
   - Equipaje (si pregunta por maletas, equipaje perdido, recogida)
   - Seguridad (si pregunta por controles, prohibiciones, normativas)
   - Transporte (si pregunta por buses, taxis, metro, cómo llegar)
   - Información General (si pregunta datos del aeropuerto, servicios, tiendas)
   - Reservas (si quiere reservar algo, hacer cita)
   - Queja (si reporta un problema, reclama algo)
   - Otros (SOLO si no encaja en ninguna categoría anterior)

2. AEROPUERTO (código IATA de 3 letras):
   Detecta el aeropuerto mencionado:
   - REU si menciona ""Reus""
   - GRO si menciona ""Girona"" o ""Costa Brava""
   - BCN si menciona ""Barcelona"" o ""El Prat""
   - MAD si menciona ""Madrid"" o ""Barajas""
   - AGP si menciona ""Málaga"" o ""Costa del Sol""
   - VLC si menciona ""Valencia"" o ""Manises""
   - PMI si menciona ""Palma"" o ""Mallorca"" o ""Son Sant Joan""
   - Y así con otros aeropuertos españoles
   - Si NO menciona ningún aeropuerto, usa ""MAD""

3. RESUMEN (máximo 2 frases):
   Resume QUÉ necesita o pregunta el cliente. NO copies la transcripción literal.

EJEMPLOS:
- ""Hola, ¿dónde está el parking del aeropuerto de Reus?"" 
  → {{""category"":""Parking"",""airportCode"":""REU"",""summary"":""Cliente consulta ubicación del parking""}}

- ""¿A qué hora sale el vuelo a Londres desde Barcelona?""
  → {{""category"":""Vuelos"",""airportCode"":""BCN"",""summary"":""Consulta horario vuelo a Londres""}}

Responde SOLO con JSON (sin ```json, sin texto extra):
{{
  ""category"": ""nombre exacto de categoría"",
  ""airportCode"": ""código de 3 letras"",
  ""summary"": ""resumen breve""
}}

TRANSCRIPCIÓN:
{transcript}";

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
