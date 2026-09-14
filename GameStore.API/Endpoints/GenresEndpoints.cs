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
    }
}
