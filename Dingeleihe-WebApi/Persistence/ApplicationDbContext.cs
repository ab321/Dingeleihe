using System.Diagnostics;
using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Persistence;

public class ApplicationDbContext : DbContext
{
    public DbSet<Image> ImageDetails { get; set; }

    public DbSet<Rental> Rentals { get; set; }
    
    public DbSet<Shelf> Shelves { get; set; }
    
    public DbSet<Thing> Things { get; set; }
    
    public DbSet<ThingDetails> ThingDetails { get; set; }
    
    public DbSet<User> Users { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if(!optionsBuilder.IsConfigured)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            optionsBuilder
                .LogTo(msg => Debug.WriteLine(msg), Microsoft.Extensions.Logging.LogLevel.Debug, Microsoft.EntityFrameworkCore.Diagnostics.DbContextLoggerOptions.SingleLine | Microsoft.EntityFrameworkCore.Diagnostics.DbContextLoggerOptions.UtcTime)
                .UseNpgsql(connectionString: configuration["ConnectionStrings:Default"]);
        }
    }
}