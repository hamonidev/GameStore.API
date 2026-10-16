using GameStore.API.Data;
using GameStore.API.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GameStore.API.Endpoints;

public static class GenresEndpoints
{
    public static void MapGenresEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/genres");

        // GET /genres
        group.MapGet("/", async (GameStoreContext dbContext) =>
            await dbContext.Genres
                            .Select(genre => new GenreDto(genre.Id, genre.Name))
                            .AsNoTracking()
                            .ToListAsync()
         );

        // GET /genres{id}
        group.MapGet("/{id}", async (int id, GameStoreContext dbContext) =>
        {
            var genre = await dbContext.Genres.FindAsync(id);

            return genre is null ? Results.Problem(
                detail: $"Genre with ID {id} was not found.",
                statusCode: StatusCodes.Status404NotFound,
                title: "Genre Not Found"
            ) : Results.Ok(new GenreDto(
                genre.Id,
                genre.Name
            ));
        });
    }
}
