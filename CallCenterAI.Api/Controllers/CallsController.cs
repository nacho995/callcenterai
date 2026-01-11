using CallCenterAI.Api.Dtos;
using CallCenterAI.Api.Models;
using CallCenterAI.Api.Services;
using CallCenterAI.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CallCenterAI.Api.Controllers;

[ApiController]
[Route("api/calls")]
public class CallsController : ControllerBase
{
    private readonly CallAiService _callAiService;
    private readonly SpeechToTextService _speechService;
    private readonly AppDbContext _db;

    public CallsController(CallAiService callAiService, SpeechToTextService speechService, AppDbContext db)
    {
        _callAiService = callAiService;
        _speechService = speechService;
        _db = db;
    }

    [HttpPost]
    public async Task<ActionResult<CallSummaryResponse>> CreateCall([FromBody] CreateCallRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Transcript))
        {
            return BadRequest("Transcript is required");
        }

        if (string.IsNullOrWhiteSpace(request.FromNumber))
        {
            return BadRequest("FromNumber is required");
        }

        var summary = await _callAiService.AnalyzeAsync(request.Transcript);
        return Ok(summary);
    }

    [HttpPost("audio")]
    public async Task<IActionResult> CreateFromAudio(
        [FromForm] CreateCallAudioRequest request,
        [FromForm] IFormFile audio)
    {
        try
        {
            Console.WriteLine($"Received audio request from employee: {request.EmployeeId}");
            
            if (audio == null || audio.Length == 0)
            {
                Console.WriteLine("ERROR: No audio file received");
                return BadRequest("Audio file is required");
            }
            
            Console.WriteLine($"Audio file size: {audio.Length} bytes");
            
            var audioPath = Path.GetTempFileName();

            using (var stream = System.IO.File.Create(audioPath))
                await audio.CopyToAsync(stream);

            Console.WriteLine("Starting transcription...");
            var transcript = await _speechService.TranscribeAsync(audioPath);
            Console.WriteLine($"Transcription complete: {transcript.Substring(0, Math.Min(50, transcript.Length))}...");

            Console.WriteLine("Starting AI analysis...");
            var analysis = await _callAiService.AnalyzeAsync(transcript);
            Console.WriteLine($"Analysis complete - Airport: {analysis.AirportCode}, Category: {analysis.Category}");

            // Buscar el aeropuerto por código
            var airport = await _db.Airports.FirstOrDefaultAsync(a => a.Code == analysis.AirportCode);
            if (airport == null)
            {
                Console.WriteLine($"ERROR: Airport not found: {analysis.AirportCode}");
                return BadRequest($"Airport desconocido: {analysis.AirportCode}. Por favor, ejecuta /api/seed primero.");
            }

            // Buscar o crear la categoría
            var category = await _db.Categories.FirstOrDefaultAsync(c => c.Name == analysis.Category);
            if (category == null)
            {
                category = new Category { Name = analysis.Category };
                _db.Categories.Add(category);
                await _db.SaveChangesAsync();
            }

            var call = new Call
            {
                EmployeeId = request.EmployeeId,
                AirportId = airport.Id,
                CategoryId = category.Id,
                Transcript = transcript,
                Summary = analysis.Summary,
                CreatedAt = DateTime.UtcNow
            };

            _db.Calls.Add(call);
            await _db.SaveChangesAsync();

            // Cargar las relaciones para la respuesta
            await _db.Entry(call).Reference(c => c.Airport).LoadAsync();
            await _db.Entry(call).Reference(c => c.Category).LoadAsync();

            Console.WriteLine($"Call saved successfully, ID: {call.Id}");
            return Ok(call);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR in CreateFromAudio: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}