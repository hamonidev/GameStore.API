using GameStore.API.DTOs;

namespace GameStore.API.Endpoints;

public static class GamesEndpoints
{
    const string GetGameEndpoint = "GetGame";

    private static readonly List<GameDto> games = [
        new (
            1,
            "Elden Ring",
            "Action RPG (ARPG)",
            49.99M,
            new DateOnly(2022, 2, 25)),
        new (
            2,
            "Black Myth: Wukong",
            "Action RPG (ARPG)",
            54.99M,
            new DateOnly(2024, 8, 20)),
        new (
            3,
            "Minecraft",
            "Survival",
            19.99M,
            new DateOnly(2011, 11, 18))
    ];

    public static void MapGamesEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/games");

        // GET /games
        group.MapGet("/", () => games);

        // GET /games/{id}
        group.MapGet("/{id}", (int id) =>
        {
            var game = games.Find(game => game.Id == id);

            return game is null ? Results.NotFound() : Results.Ok(game);
        })
        .WithName(GetGameEndpoint);

        // POST /games
        group.MapPost("/", (CreateGameDto newGame) =>
        {
            GameDto game = new(
                games.Count + 1,
                newGame.Name,
                newGame.Genre,
                newGame.Price,
                newGame.ReleaseDate
            );

            games.Add(game);

            return Results.CreatedAtRoute(GetGameEndpoint, new { id = game.Id }, game);
        });

        // PUT /games/{id}
        group.MapPut("/{id}", (int id, UpdateGameDto updatedGame) =>
        {
            var index = games.FindIndex(game => game.Id == id);

            if (index == -1)
            {
                return Results.NotFound();
            }

            games[index] = new GameDto(
                id,
                updatedGame.Name,
                updatedGame.Genre,
                updatedGame.Price,
                updatedGame.ReleaseDate
            );

            return Results.NoContent();
        });

        // DELETE /games/{id}
        group.MapDelete("/{id}", (int id) =>
        {
            games.RemoveAll(game => game.Id == id);

            return Results.NoContent();
        });
    }

}

