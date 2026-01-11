using CallCenterAI.Api.BackgroundJobs;
using CallCenterAI.Api.Data;
using CallCenterAI.Api.Models;
using CallCenterAI.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace CallCenterAI.Api.BackgroundJobs;

public class DailySummaryJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public DailySummaryJob(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var ai = scope.ServiceProvider.GetRequiredService<CallAiService>();
        var today = DateTime.UtcNow.Date;
        var calls = await db.Calls.Where(c => c.CreatedAt.Date == today).GroupBy(c => c.EmployeeId).ToListAsync();
        foreach (var group in calls)
        {
            var text = string.Join("\n", group.Select(c => c.Summary));

            var summary = await ai.AnalyzeAsync(
                $"Resumen diario de llamadas:\n{text}"
            );
            db.DailySummaries.Add(new DailySummary
            {
                EmployeeId = group.Key,
                Date = today,
                Summary = summary.Summary
            });
            await db.SaveChangesAsync();
        }
    }
}
