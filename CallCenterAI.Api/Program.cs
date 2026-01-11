/*
 * CallCenter AI - Sistema Inteligente de Análisis de Llamadas
 * Copyright (c) 2026 - Todos los derechos reservados
 * Uso no autorizado está estrictamente prohibido
 */

using CallCenterAI.Api.Services;
using CallCenterAI.Api.Data;
using CallCenterAI.Api.Models;
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

// Seed endpoint para poblar aeropuertos (GET para acceder desde navegador)
app.MapGet("/api/seed", async (AppDbContext db) =>
{
    try
    {
        if (await db.Airports.AnyAsync())
        {
            return Results.Ok(new { message = "Database already seeded", count = await db.Airports.CountAsync() });
        }

        // Primero, arreglar las secuencias de PostgreSQL para auto-incremento
        await db.Database.ExecuteSqlRawAsync(@"
            DO $$ 
            BEGIN
                IF NOT EXISTS (SELECT 1 FROM pg_sequences WHERE schemaname = 'public' AND sequencename = 'Airports_Id_seq') THEN
                    CREATE SEQUENCE ""Airports_Id_seq"";
                    ALTER TABLE ""Airports"" ALTER COLUMN ""Id"" SET DEFAULT nextval('""Airports_Id_seq""');
                    ALTER SEQUENCE ""Airports_Id_seq"" OWNED BY ""Airports"".""Id"";
                END IF;
                
                IF NOT EXISTS (SELECT 1 FROM pg_sequences WHERE schemaname = 'public' AND sequencename = 'Categories_Id_seq') THEN
                    CREATE SEQUENCE ""Categories_Id_seq"";
                    ALTER TABLE ""Categories"" ALTER COLUMN ""Id"" SET DEFAULT nextval('""Categories_Id_seq""');
                    ALTER SEQUENCE ""Categories_Id_seq"" OWNED BY ""Categories"".""Id"";
                END IF;
                
                IF NOT EXISTS (SELECT 1 FROM pg_sequences WHERE schemaname = 'public' AND sequencename = 'Calls_Id_seq') THEN
                    CREATE SEQUENCE ""Calls_Id_seq"";
                    ALTER TABLE ""Calls"" ALTER COLUMN ""Id"" SET DEFAULT nextval('""Calls_Id_seq""');
                    ALTER SEQUENCE ""Calls_Id_seq"" OWNED BY ""Calls"".""Id"";
                END IF;
                
                IF NOT EXISTS (SELECT 1 FROM pg_sequences WHERE schemaname = 'public' AND sequencename = 'DailySummaries_Id_seq') THEN
                    CREATE SEQUENCE ""DailySummaries_Id_seq"";
                    ALTER TABLE ""DailySummaries"" ALTER COLUMN ""Id"" SET DEFAULT nextval('""DailySummaries_Id_seq""');
                    ALTER SEQUENCE ""DailySummaries_Id_seq"" OWNED BY ""DailySummaries"".""Id"";
                END IF;
            END $$;
        ");

        // Todos los aeropuertos comerciales de España
        var airports = new[]
        {
            // Península - Principales
            ("MAD", "Madrid-Barajas Adolfo Suárez"),
            ("BCN", "Barcelona-El Prat Josep Tarradellas"),
            ("AGP", "Málaga-Costa del Sol"),
            ("VLC", "Valencia"),
            ("SVQ", "Sevilla"),
            ("ALC", "Alicante-Elche"),
            ("BIO", "Bilbao"),
            
            // Península - Secundarios
            ("LEI", "Almería"),
            ("OVD", "Asturias"),
            ("GRX", "Granada-Federico García Lorca"),
            ("SDR", "Santander"),
            ("SCQ", "Santiago de Compostela"),
            ("VGO", "Vigo-Peinador"),
            ("VLL", "Valladolid"),
            ("ZAZ", "Zaragoza"),
            ("RGS", "Burgos"),
            ("BJZ", "Badajoz"),
            ("LCG", "A Coruña"),
            ("SLM", "Salamanca"),
            ("VIT", "Vitoria"),
            ("PNA", "Pamplona"),
            ("RJL", "Logroño-Agoncillo"),
            ("HSK", "Huesca-Pirineos"),
            ("RMU", "Región de Murcia"),
            ("XRY", "Jerez de la Frontera"),
            ("LEN", "León"),
            ("REU", "Reus"),
            ("GRO", "Girona-Costa Brava"),
            
            // Baleares
            ("PMI", "Palma de Mallorca"),
            ("IBZ", "Ibiza"),
            ("MAH", "Menorca"),
            
            // Canarias
            ("LPA", "Gran Canaria"),
            ("TFS", "Tenerife Sur"),
            ("TFN", "Tenerife Norte"),
            ("ACE", "Lanzarote"),
            ("FUE", "Fuerteventura"),
            ("GMZ", "La Gomera"),
            ("VDE", "El Hierro"),
            ("SPC", "La Palma"),
            
            // Ceuta y Melilla
            ("JCU", "Ceuta Heliport"),
            ("MLN", "Melilla")
        };

        foreach (var (code, name) in airports)
        {
            await db.Database.ExecuteSqlRawAsync(
                "INSERT INTO \"Airports\" (\"Code\", \"Name\") VALUES ({0}, {1})",
                code, name);
        }

        var count = await db.Airports.CountAsync();
        return Results.Ok(new { message = "Database seeded successfully", count });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error seeding database: {ex.Message}");
    }
});

app.MapControllers();

// Usar el puerto de la variable de entorno PORT (Render usa 10000)
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Add($"http://0.0.0.0:{port}");

app.Run();
