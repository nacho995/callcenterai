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

// Configurar CORS de forma más explícita para producción
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.SetIsOriginAllowed(_ => true)  // Permite cualquier origen
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
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
        {
            opt.UseNpgsql(connectionString);
            // Suprimir warning de pending changes en producción
            opt.ConfigureWarnings(warnings => 
                warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"ERROR converting DATABASE_URL: {ex.Message}");
        Console.WriteLine("Falling back to SQLite");
        builder.Services.AddDbContext<AppDbContext>(opt =>
            opt.UseSqlite("Data Source=/tmp/callcenter.db"));
    }
}

// Deshabilitar temporalmente el DailySummaryJob hasta arreglar queries PostgreSQL
// builder.Services.AddHostedService<DailySummaryJob>();


var app = builder.Build();

// Seed database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        await db.Database.MigrateAsync();
        Console.WriteLine("Database migrations applied successfully");
        
        // Temporalmente deshabilitado el seeder - tiene problemas con PostgreSQL
        // await CallCenterAI.Api.Data.DbSeeder.SeedAsync(db);
        Console.WriteLine("Database seeding skipped");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database initialization error: {ex.Message}");
        // Continuar de todas formas para que la app arranque
    }
}

// CORS debe ser lo primero
app.UseCors("AllowAll");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseHttpsRedirection();

// Health check endpoint
app.MapGet("/health", () => "OK");

// Seed endpoint para poblar aeropuertos
app.MapPost("/api/seed", async (AppDbContext db) =>
{
    if (await db.Airports.AnyAsync())
    {
        return Results.Ok(new { message = "Database already seeded" });
    }

    var airports = new List<Airport>
    {
        new() { Code = "MAD", Name = "Madrid-Barajas Adolfo Suárez" },
        new() { Code = "BCN", Name = "Barcelona-El Prat Josep Tarradellas" },
        new() { Code = "AGP", Name = "Málaga-Costa del Sol" },
        new() { Code = "PMI", Name = "Palma de Mallorca" },
        new() { Code = "VLC", Name = "Valencia" },
        new() { Code = "SVQ", Name = "Sevilla" },
        new() { Code = "ALC", Name = "Alicante-Elche" },
        new() { Code = "BIO", Name = "Bilbao" },
        new() { Code = "LPA", Name = "Gran Canaria" },
        new() { Code = "TFS", Name = "Tenerife Sur" }
    };

    await db.Airports.AddRangeAsync(airports);
    await db.SaveChangesAsync();

    return Results.Ok(new { message = "Database seeded successfully", count = airports.Count });
});

app.MapControllers();

// Usar el puerto de la variable de entorno PORT (Render usa 10000)
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Add($"http://0.0.0.0:{port}");

app.Run();
