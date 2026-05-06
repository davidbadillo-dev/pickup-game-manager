using Microsoft.EntityFrameworkCore;
using PickupGameManager.Web.Models;

namespace PickupGameManager.Web.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Game> Games => Set<Game>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var game = modelBuilder.Entity<Game>();
        game.HasKey(g => g.Id);
        game.Property(g => g.Title).HasMaxLength(200).IsRequired();
        game.Property(g => g.Location).HasMaxLength(500).IsRequired();
        game.Property(g => g.MaxPlayers).IsRequired();
    }
}
