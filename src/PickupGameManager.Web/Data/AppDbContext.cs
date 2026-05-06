using Microsoft.EntityFrameworkCore;
using PickupGameManager.Web.Models;

namespace PickupGameManager.Web.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Game> Games => Set<Game>();
    public DbSet<Participant> Participants => Set<Participant>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var game = modelBuilder.Entity<Game>();
        game.HasKey(g => g.Id);
        game.Property(g => g.Title).HasMaxLength(200).IsRequired();
        game.Property(g => g.Location).HasMaxLength(500).IsRequired();
        game.Property(g => g.MaxPlayers).IsRequired();

        var participant = modelBuilder.Entity<Participant>();
        participant.HasKey(p => p.Id);
        participant.Property(p => p.Name).HasMaxLength(200).IsRequired();
        participant.Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(32);
        participant.HasOne(p => p.Game)
            .WithMany(g => g.Participants)
            .HasForeignKey(p => p.GameId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
