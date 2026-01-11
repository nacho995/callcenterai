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

// Render usa DATABASE_URL en formato URI, necesita conversión para Npgsql
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

if (string.IsNullOrEmpty(databaseUrl))
{
    Console.WriteLine("ERROR: DATABASE_URL not found! Using SQLite fallback");
    builder.Services.AddDbContext<AppDbContext>(opt =>
        opt.UseSqlite("Data Source=/tmp/callcenter.db"));
}
else
{
    Console.WriteLine($"DATABASE_URL found, length: {databaseUrl.Length}");
    
    // Convertir de formato URI a formato Npgsql
    try
    {
        var databaseUri = new Uri(databaseUrl);
        var userInfo = databaseUri.UserInfo.Split(':');
        
        var connectionString = $"Host={databaseUri.Host};" +
                             $"Port={databaseUri.Port};" +
                             $"Username={userInfo[0]};" +
                             $"Password={userInfo[1]};" +
                             $"Database={databaseUri.LocalPath.TrimStart('/')};" +
                             $"SSL Mode=Require;" +
                             $"Trust Server Certificate=true";
        
        Console.WriteLine("Converted to Npgsql format successfully");
        builder.Services.AddDbContext<AppDbContext>(opt =>
            opt.UseNpgsql(connectionString));
    }
    catch (Exception ex)
    {
        Console.WriteLine($"ERROR converting DATABASE_URL: {ex.Message}");
        Console.WriteLine("Falling back to SQLite");
        builder.Services.AddDbContext<AppDbContext>(opt =>
            opt.UseSqlite("Data Source=/tmp/callcenter.db"));
    }
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
