using GameStore.API.Models;
using Microsoft.EntityFrameworkCore;

namespace GameStore.API.Data;

public class GameStoreContext(DbContextOptions<GameStoreContext> options)
    : DbContext(options)
{
    public DbSet<Game> Games => Set<Game>();

    public DbSet<Genre> Genres => Set<Genre>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Game>(entity =>
        {
            entity.HasIndex(game => game.Name).IsUnique();
        });
    }
}
