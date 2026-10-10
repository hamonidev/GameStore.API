using System.Net;
using System.Net.Http.Json;
using GameStore.API.DTOs;

namespace GameStore.API.Tests;

public class UnitTest1 : IClassFixture<GameStoreApiFactory>
{
    private readonly GameStoreApiFactory _factory;

    public UnitTest1(GameStoreApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetGame_NonExistingId_ReturnsNotFound()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/games/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateGame_ValidGame_ReturnsCreated()
    {
        using var client = _factory.CreateClient();

        var newGame = new CreateGameDto(
            "Test Game",
            1,
            29.99M,
            new DateOnly(2026, 1, 1));

        var response = await client.PostAsJsonAsync("/games", newGame);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var createdGame = await response.Content.ReadFromJsonAsync<GameDetailsDto>();

        Assert.NotNull(createdGame);
        Assert.Equal("Test Game", createdGame.Name);
        Assert.Equal(1, createdGame.GenreId);
        Assert.Equal(29.99M, createdGame.Price);
    }

    [Fact]
    public async Task CreateGame_DuplicateName_ReturnsConflict()
    {
        using var client = _factory.CreateClient();

        var newGame = new CreateGameDto(
            "Duplicate Test",
            1,
            29.99M,
            new DateOnly(2026, 1, 1));

        var response = await client.PostAsJsonAsync("/games", newGame);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var duplicateResponse = await client.PostAsJsonAsync("/games", newGame);

        Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);
    }
}

