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
        var audioPath = Path.GetTempFileName();

        using (var stream = System.IO.File.Create(audioPath))
            await audio.CopyToAsync(stream);

        var transcript = await _speechService.TranscribeAsync(audioPath);
        var analysis = await _callAiService.AnalyzeAsync(transcript);

        // Buscar el aeropuerto por código
        var airport = await _db.Airports.FirstOrDefaultAsync(a => a.Code == analysis.AirportCode);
        if (airport == null)
            return BadRequest($"Airport desconocido: {analysis.AirportCode}");

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

        return Ok(call);
    }
}