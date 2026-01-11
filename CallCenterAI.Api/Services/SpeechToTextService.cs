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
            throw new InvalidOperationException("OpenAI API Key not configured");
        }

        var client = new OpenAI.OpenAIClient(apiKey);
        var audioClient = client.GetAudioClient("whisper-1");

        using var audioFileStream = File.OpenRead(audioPath);
        
        var transcription = await audioClient.TranscribeAudioAsync(
            audioFileStream,
            Path.GetFileName(audioPath),
            new AudioTranscriptionOptions
            {
                Language = "es",
                ResponseFormat = AudioTranscriptionFormat.Text,
                // Agregar contexto para mejorar precisión con términos aeroportuarios
                Prompt = "Esta es una llamada de un call center de aeropuertos españoles. Incluye términos como: aeropuerto, Madrid, Barcelona, Valencia, vuelo, equipaje, reserva, información."
            });

        return transcription.Value.Text;
    }
}