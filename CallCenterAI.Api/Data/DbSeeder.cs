using CallCenterAI.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CallCenterAI.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (!await db.Airports.AnyAsync())
        {
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
        }
    }
}
