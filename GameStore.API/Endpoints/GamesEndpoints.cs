using GameStore.API.Data;
using GameStore.API.DTOs;
using GameStore.API.Models;
using Microsoft.EntityFrameworkCore;

namespace GameStore.API.Endpoints;

public static class GamesEndpoints
{
    const string GetGameEndpoint = "GetGame";

    public static void MapGamesEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/games");

        // GET /games & /games?genreId={id}
        group.MapGet("/", async (int? genreId, string? search, GameStoreContext dbContext) =>
        {
            IQueryable<Game> query = dbContext.Games;

            if (genreId.HasValue)
            {
                query = query.Where(game => game.GenreId == genreId.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(game => game.Name.Contains(search));
            }

            return await query
                .Select(game => new GameSummaryDto(
                  game.Id,
                  game.Name,
                  game.Genre!.Name,
                  game.Price,
                  game.ReleaseDate))
                  .AsNoTracking()
                  .ToListAsync();
        });

        // GET /games/{id}
        group.MapGet("/{id}", async (int id, GameStoreContext dbContext) =>
        {
            var game = await dbContext.Games.FindAsync(id);

            return game is null ? Results.NotFound() : Results.Ok(new GameDetailsDto(
                game.Id,
                game.Name,
                game.GenreId,
                game.Price,
                game.ReleaseDate
            ));
        })
        .WithName(GetGameEndpoint);

        // POST /games
        group.MapPost("/", async (CreateGameDto newGame, GameStoreContext dbContext) =>
        {
            bool genreExists = await dbContext.Genres.AnyAsync(genre => genre.Id == newGame.GenreId);

            if (!genreExists)
                return Results.BadRequest($"The Genre ID {newGame.GenreId} does not exist.");

            Game game = new()
            {
                Name = newGame.Name,
                GenreId = newGame.GenreId,
                Price = newGame.Price,
                ReleaseDate = newGame.ReleaseDate
            };

            dbContext.Games.Add(game);
            await dbContext.SaveChangesAsync();

            GameDetailsDto gameDto = new(
                game.Id,
                game.Name,
                game.GenreId,
                game.Price,
                game.ReleaseDate
            );

            return Results.CreatedAtRoute(GetGameEndpoint, new { id = gameDto.Id }, gameDto);
        });

        // PUT /games/{id}
        group.MapPut("/{id}", async (int id, UpdateGameDto updatedGame, GameStoreContext dbContext) =>
        {
            var existingGame = await dbContext.Games.FindAsync(id);

            if (existingGame is null)
            {
                return Results.NotFound();
            }

            existingGame.Name = updatedGame.Name;
            existingGame.GenreId = updatedGame.GenreId;
            existingGame.Price = updatedGame.Price;
            existingGame.ReleaseDate = updatedGame.ReleaseDate;

            await dbContext.SaveChangesAsync();

            return Results.NoContent();
        });

        // DELETE /games/{id}
        group.MapDelete("/{id}", async (int id, GameStoreContext dbContext) =>
        {
            var deletedCount = await dbContext.Games
                .Where(game => game.Id == id)
                .ExecuteDeleteAsync();

            if (deletedCount == 0)
            {
                return Results.NotFound($"Game with ID {id} was not found.");
            }

            return Results.NoContent();
        });
    }

}

