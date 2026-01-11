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
        Console.WriteLine($"📥 Analyzing transcript ({transcript.Length} chars)");
        
        var prompt = $@"Analiza esta llamada de call center y extrae información estructurada.

CATEGORÍAS DISPONIBLES (elige la más específica):
• Parking - aparcamiento, estacionamiento, tarifas parking
• Vuelos - horarios, salidas, llegadas, retrasos, información de vuelos
• Facturación - check-in, facturar equipaje, mostrador
• Equipaje - maletas, equipaje perdido, recogida equipaje
• Seguridad - controles, prohibiciones, artículos prohibidos
• Transporte - buses, taxis, metro, tren, cómo llegar al aeropuerto
• Información General - servicios aeropuerto, tiendas, restaurantes, wifi
• Reservas - hacer reservas, citas
• Queja - problemas, reclamos, incidencias
• Otros - cualquier otra consulta

AEROPUERTOS ESPAÑOLES (código IATA):
REU=Reus, GRO=Girona, BCN=Barcelona, MAD=Madrid, AGP=Málaga, VLC=Valencia,
SVQ=Sevilla, ALC=Alicante, BIO=Bilbao, PMI=Palma, IBZ=Ibiza, MAH=Menorca,
LPA=Gran Canaria, TFS=Tenerife Sur, TFN=Tenerife Norte, ACE=Lanzarote

INSTRUCCIONES:
1. Identifica el aeropuerto mencionado (si no hay ninguno, usa MAD)
2. Clasifica en la categoría MÁS ESPECÍFICA
3. Resume en 1-2 frases QUÉ quiere el cliente (NO copies el texto literal)

EJEMPLOS:
""Hola, ¿dónde está el parking de Reus?"" →
{{""category"":""Parking"",""airportCode"":""REU"",""summary"":""Consulta ubicación del parking""}}

""¿A qué hora sale el vuelo a Londres desde Barcelona?"" →
{{""category"":""Vuelos"",""airportCode"":""BCN"",""summary"":""Solicita horario de vuelo a Londres""}}

""¿Cuánto cuesta aparcar en el aeropuerto de Málaga?"" →
{{""category"":""Parking"",""airportCode"":""AGP"",""summary"":""Pregunta tarifas de aparcamiento""}}

Responde ÚNICAMENTE con JSON válido (sin ```json, sin comentarios):

TRANSCRIPCIÓN A ANALIZAR:
{transcript}";

        var messages = new List<ChatMessage>
        {
            ChatMessage.CreateSystemMessage("Eres un experto en análisis de llamadas. Extrae información clave y genera resúmenes concisos. Responde ÚNICAMENTE con JSON sin formato markdown ni explicaciones adicionales."),
            ChatMessage.CreateUserMessage(prompt)
        };

        var chatOptions = new ChatCompletionOptions
        {
            Temperature = 0.2f, // Muy determinístico para reducir variación
            MaxOutputTokenCount = 250,
            TopP = 0.95f
        };

        Console.WriteLine($"🔄 Calling OpenAI GPT ({_model})...");
        var startTime = DateTime.UtcNow;
        
        var response = await _client.CompleteChatAsync(messages, chatOptions);
        var elapsed = (DateTime.UtcNow - startTime).TotalSeconds;
        
        var jsonText = response.Value.Content[0].Text.Trim();
        
        Console.WriteLine($"⏱️  GPT response time: {elapsed:F2}s");
        Console.WriteLine($"📊 Response length: {jsonText.Length} characters");
        
        // Limpiar markdown si viene con ```json o ```
        if (jsonText.Contains("```"))
        {
            Console.WriteLine("🧹 Cleaning markdown from response...");
            // Eliminar ```json o ``` del inicio y final
            jsonText = System.Text.RegularExpressions.Regex.Replace(jsonText, @"```(json)?\s*", "");
            jsonText = jsonText.Trim();
        }

        Console.WriteLine("");
        Console.WriteLine("=== AI RAW RESPONSE (cleaned) ===");
        Console.WriteLine(jsonText);
        Console.WriteLine("=".PadRight(60, '='));
        Console.WriteLine("");
        
        CallSummaryResponse? result;
        try
        {
            var options = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,  // Ignorar mayúsculas/minúsculas
                AllowTrailingCommas = true
            };
            
            result = System.Text.Json.JsonSerializer.Deserialize<CallSummaryResponse>(jsonText, options);
            
            if (result == null)
            {
                Console.WriteLine($"❌ ERROR: Deserialization returned null");
                throw new Exception("Deserialization returned null");
            }
            
            Console.WriteLine($"✅ JSON parsed successfully");
            Console.WriteLine($"   📂 Category: '{result.Category}'");
            Console.WriteLine($"   ✈️  Airport: '{result.AirportCode}'");
            Console.WriteLine($"   📝 Summary: '{result.Summary}'");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ ERROR parsing AI response: {ex.Message}");
            Console.WriteLine($"   Problematic JSON: {jsonText}");
            Console.WriteLine($"   Full exception: {ex}");
            
            // Fallback robusto
            result = new CallSummaryResponse
            {
                Category = "Otros",
                AirportCode = "MAD",
                Summary = transcript.Length > 100 
                    ? $"{transcript.Substring(0, 97)}..." 
                    : transcript
            };
            Console.WriteLine($"🔧 Using fallback values");
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
