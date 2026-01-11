using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using OpenAI.Audio;

namespace CallCenterAI.Api.Services;

public class SpeechToTextService
{
    private readonly IConfiguration _config;

    public SpeechToTextService(IConfiguration config)
    {
        _config = config;
    }

    public async Task<string> TranscribeAsync(string audioPath)
    {
        var apiKey = _config["OpenAI:ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
        {
            Console.WriteLine("❌ ERROR: OpenAI API Key not configured");
            throw new InvalidOperationException("OpenAI API Key not configured");
        }

        // Verificar que el archivo existe
        if (!File.Exists(audioPath))
        {
            Console.WriteLine($"❌ ERROR: Audio file not found: {audioPath}");
            throw new FileNotFoundException($"Audio file not found: {audioPath}");
        }

        var fileInfo = new FileInfo(audioPath);
        Console.WriteLine($"📂 Opening file: {fileInfo.Name} ({fileInfo.Length:N0} bytes)");

        var client = new OpenAI.OpenAIClient(apiKey);
        var audioClient = client.GetAudioClient("whisper-1");

        using var audioFileStream = File.OpenRead(audioPath);
        
        Console.WriteLine($"🔄 Sending to Whisper API...");
        var startTime = DateTime.UtcNow;
        
        var transcription = await audioClient.TranscribeAudioAsync(
            audioFileStream,
            Path.GetFileName(audioPath),
            new AudioTranscriptionOptions
            {
                Language = "es",
                ResponseFormat = AudioTranscriptionFormat.Verbose,
                Temperature = 0.0f,  // Más determinístico, menos alucinaciones
                // Prompt corto y específico para reducir alucinaciones
                Prompt = "Llamada en español sobre aeropuertos: parking, vuelos, equipaje, facturación, información."
            });

        var elapsed = (DateTime.UtcNow - startTime).TotalSeconds;
        var text = transcription.Value.Text.Trim();
        var duration = transcription.Value.Duration?.TotalSeconds ?? 0;
        var language = transcription.Value.Language ?? "unknown";
        
        Console.WriteLine($"⏱️  Whisper API response time: {elapsed:F2}s");
        Console.WriteLine($"🎵 Audio duration: {duration:F1}s");
        Console.WriteLine($"🌐 Detected language: {language}");
        Console.WriteLine($"📊 Transcription length: {text.Length} characters");
        
        if (string.IsNullOrWhiteSpace(text))
        {
            Console.WriteLine("⚠️  WARNING: Whisper returned empty transcription");
        }
        else if (text.Contains("Gracias por ver") || text.Contains("suscrib") || text.Contains("vídeo"))
        {
            Console.WriteLine("⚠️  WARNING: Detected hallucination pattern (YouTube phrases)");
            Console.WriteLine("💡 This usually means the audio is corrupted, too short, or silent");
        }
        
        Console.WriteLine($"📝 Full transcription: \"{text}\"");
        
        return text;
    }
}