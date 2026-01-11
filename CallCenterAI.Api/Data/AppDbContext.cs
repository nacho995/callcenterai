using Microsoft.EntityFrameworkCore;
using CallCenterAI.Api.Models;

namespace CallCenterAI.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    public DbSet<Airport> Airports => Set<Airport>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Call> Calls => Set<Call>();
    public DbSet<DailySummary> DailySummaries => Set<DailySummary>();
}