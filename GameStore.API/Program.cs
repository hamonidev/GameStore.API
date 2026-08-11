using GameStore.API.DTOs;

const string GetGameEndpoint = "GetGame";

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

List<GameDto> games = [
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

// GET /games
app.MapGet("/games", () => games);


// GET /games/{id}
app.MapGet("/games/{id}", (int id) => games.Find(game => game.Id == id))
    .WithName(GetGameEndpoint);

// POST /games
app.MapPost("/games", (CreateGameDto newGame) =>
{
    GameDto game = new (
        games.Count + 1,
        newGame.Name,
        newGame.Genre,
        newGame.Price,
        newGame.ReleaseDate
    );

    games.Add(game);

    return Results.CreatedAtRoute(GetGameEndpoint, new {id = game.Id}, game);
});

// PUT /games/{id}
app.MapPut("/games/{id}", (int id, UpdateGameDto updatedGame) =>
{
    var index = games.FindIndex(game => game.Id == id);

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
app.MapDelete("/games/{id}", (int id) =>
{
    games.RemoveAll(game => game.Id == id);

    return Results.NoContent();
});

app.Run();
