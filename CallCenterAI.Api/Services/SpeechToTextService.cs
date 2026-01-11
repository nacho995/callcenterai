using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;

namespace CallCenterAI.Api.Services;

public class SpeechToTextService
{
    private readonly HttpClient _http;
    private readonly string _baseUrl;

    public SpeechToTextService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _baseUrl = config["SpeechService:BaseUrl"] ?? "http://localhost:8000";
    }

    public async Task<string> TranscribeAsync(string audioPath)
    {
        using var content = new MultipartFormDataContent();
        var bytes = await File.ReadAllBytesAsync(audioPath);
        var fileContent = new ByteArrayContent(bytes);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("audio/wav");

        content.Add(fileContent, "audio", Path.GetFileName(audioPath));

        var response = await _http.PostAsync(
            $"{_baseUrl}/transcribe",
            content
        );
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        return System.Text.Json.JsonDocument
            .Parse(json)
            .RootElement
            .GetProperty("text")
            .GetString()!;
    }
}