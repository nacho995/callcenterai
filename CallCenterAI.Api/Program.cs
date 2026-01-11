/*
 * CallCenter AI - Sistema Inteligente de Análisis de Llamadas
 * Copyright (c) 2026 - Todos los derechos reservados
 * Uso no autorizado está estrictamente prohibido
 */

using CallCenterAI.Api.Services;
using CallCenterAI.Api.Data;
using CallCenterAI.Api.BackgroundJobs;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddScoped<CallAiService>();
builder.Services.AddScoped<SpeechToTextService>();

// Render usa DATABASE_URL por defecto
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");

Console.WriteLine($"DATABASE_URL found: {!string.IsNullOrEmpty(connectionString)}");
if (!string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine($"Connection string length: {connectionString.Length}");
    // Imprimir TODA la connection string para debug
    Console.WriteLine($"FULL Connection string: {connectionString}");
}

if (string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("ERROR: DATABASE_URL not found! Using SQLite fallback");
    builder.Services.AddDbContext<AppDbContext>(opt =>
        opt.UseSqlite("Data Source=/tmp/callcenter.db"));
}
else
{
    Console.WriteLine("Using PostgreSQL from DATABASE_URL");
    builder.Services.AddDbContext<AppDbContext>(opt =>
        opt.UseNpgsql(connectionString));
}

builder.Services.AddHostedService<DailySummaryJob>();


var app = builder.Build();

// Seed database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await CallCenterAI.Api.Data.DbSeeder.SeedAsync(db);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();
app.UseHttpsRedirection();
app.MapControllers();

app.Run();
