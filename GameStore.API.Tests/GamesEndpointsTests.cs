using System.Net;
using System.Net.Http.Json;
using GameStore.API.DTOs;

namespace GameStore.API.Tests;

public class GamesEndpointsTests
{
    [Fact]
    public async Task CreateGame_ValidGame_ReturnsCreated()
    {
        using var factory = new GameStoreApiFactory();
        using var client = factory.CreateClient();

        var newGame = new CreateGameDto(
            "Valid Game",
            1,
            29.99M,
            new DateOnly(2026, 1, 1));

        var response = await client.PostAsJsonAsync("/games", newGame);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var createdGame = await response.Content.ReadFromJsonAsync<GameDetailsDto>();

        Assert.NotNull(createdGame);
        Assert.Equal("Valid Game", createdGame.Name);
        Assert.Equal(1, createdGame.GenreId);
        Assert.Equal(29.99M, createdGame.Price);
    }

    [Fact]
    public async Task CreateGame_DuplicateName_ReturnsConflict()
    {
        using var factory = new GameStoreApiFactory();
        using var client = factory.CreateClient();

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

    [Fact]
    public async Task CreateGame_InvalidGenre_ReturnsBadRequest()
    {
        using var factory = new GameStoreApiFactory();
        using var client = factory.CreateClient();

        var newGame = new CreateGameDto(
            "Invalid Genre",
            999999,
            29.99M,
            new DateOnly(2026, 1, 1));

        var response = await client.PostAsJsonAsync("/games", newGame);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateGame_DuplicateNameDifferentCasing_ReturnsConflict()
    {
        using var factory = new GameStoreApiFactory();
        using var client = factory.CreateClient();

        var firstGame = new CreateGameDto(
            "Minecraft",
            1,
            29.99M,
            new DateOnly(2026, 1, 1));

        var firstPostResponse = await client.PostAsJsonAsync("/games", firstGame);
        Assert.Equal(HttpStatusCode.Created, firstPostResponse.StatusCode);

        var secondGame = new CreateGameDto(
            "minecraft",
            1,
            29.99M,
            new DateOnly(2026, 1, 1));

        var secondPostResponse = await client.PostAsJsonAsync("/games", secondGame);
        Assert.Equal(HttpStatusCode.Conflict, secondPostResponse.StatusCode);
    }

    [Fact]
    public async Task CreateGame_ReturnsCreatedLocation()
    {
        using var factory = new GameStoreApiFactory();
        using var client = factory.CreateClient();

        var firstGame = new CreateGameDto(
            "Valid Game",
            1,
            29.99M,
            new DateOnly(2026, 1, 1));

        var postResponse = await client.PostAsJsonAsync("/games", firstGame);
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);

        var locationHeader = postResponse.Headers.Location;
        Assert.NotNull(locationHeader);

        var createdGame = await postResponse.Content.ReadFromJsonAsync<GameSummaryDto>();
        Assert.NotNull(createdGame);

        Assert.EndsWith($"/games/{createdGame.Id}", locationHeader.ToString());
    }

    [Fact]
    public async Task GetGame_NonExistingId_ReturnsNotFound()
    {
        using var factory = new GameStoreApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/games/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetGames_GenreId_ReturnsOnlyMatchingGames()
    {
        using var factory = new GameStoreApiFactory();
        using var client = factory.CreateClient();

        var gamesToCreate = new List<CreateGameDto>
        {
            new("Fantasy Adventure", 2, 49.99M, new DateOnly(2026, 1, 1)),
            new("Street Racing", 3, 39.99M, new DateOnly(2026, 1, 1)),
        };

        foreach (var gameDto in gamesToCreate)
        {
            var postResponse = await client.PostAsJsonAsync("/games", gameDto);
            Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
        }

        var getResponse = await client.GetAsync($"/games?genreId=2");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var games = await getResponse.Content.ReadFromJsonAsync<List<GameSummaryDto>>();

        Assert.NotNull(games);
        Assert.Contains(games, g => g.Name == "Fantasy Adventure");
        Assert.DoesNotContain(games, g => g.Name == "Street Racing");
        Assert.All(games, game => Assert.Equal("RPG", game.Genre));
    }

    [Fact]
    public async Task UpdateGame_NonExistingId_ReturnsNotFound()
    {
        using var factory = new GameStoreApiFactory();
        using var client = factory.CreateClient();

        var updateGame = new UpdateGameDto(
            "Updated Game",
            1,
            29.99M,
            new DateOnly(2026, 1, 1));

        var response = await client.PutAsJsonAsync("/games/99999", updateGame);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateGame_DuplicateName_ReturnsConflict()
    {
        using var factory = new GameStoreApiFactory();
        using var client = factory.CreateClient();

        var firstGame = new CreateGameDto(
            "First Game",
            1,
            29.99M,
            new DateOnly(2026, 1, 1));

        var secondGame = new CreateGameDto(
            "Second Game",
            1,
            29.99M,
            new DateOnly(2026, 1, 1));

        var firstResponse = await client.PostAsJsonAsync("/games", firstGame);
        var secondResponse = await client.PostAsJsonAsync("/games", secondGame);

        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Created, secondResponse.StatusCode);

        var firstCreatedGame = await firstResponse.Content.ReadFromJsonAsync<GameDetailsDto>();
        var secondCreatedGame = await secondResponse.Content.ReadFromJsonAsync<GameDetailsDto>();

        Assert.NotNull(firstCreatedGame);
        Assert.NotNull(secondCreatedGame);

        var updateGame = new UpdateGameDto(
            firstCreatedGame.Name,
            1,
            29.99M,
            new DateOnly(2026, 1, 1));

        var updatedResponse = await client.PutAsJsonAsync($"/games/{secondCreatedGame.Id}", updateGame);

        Assert.Equal(HttpStatusCode.Conflict, updatedResponse.StatusCode);
    }

    [Fact]
    public async Task UpdateGame_ValidGame_ReturnsNoContent()
    {
        using var factory = new GameStoreApiFactory();
        using var client = factory.CreateClient();

        var firstGame = new CreateGameDto(
            "First Game",
            1,
            29.99M,
            new DateOnly(2026, 1, 1));

        var response = await client.PostAsJsonAsync("/games", firstGame);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var createdGame = await response.Content.ReadFromJsonAsync<GameDetailsDto>();

        Assert.NotNull(createdGame);

        var updateGame = new UpdateGameDto(
            "Update Game",
            1,
            29.99M,
            new DateOnly(2026, 1, 1));

        var updateResponse = await client.PutAsJsonAsync($"/games/{createdGame.Id}", updateGame);

        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);
    }

    [Fact]
    public async Task UpdateGame_ValidGame_UpdatesGameDetails()
    {
        using var factory = new GameStoreApiFactory();
        using var client = factory.CreateClient();

        var firstGame = new CreateGameDto(
            "First Game",
            1,
            29.99M,
            new DateOnly(2026, 1, 1));

        var response = await client.PostAsJsonAsync("/games", firstGame);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var createdGame = await response.Content.ReadFromJsonAsync<GameDetailsDto>();

        Assert.NotNull(createdGame);

        var updateGame = new UpdateGameDto(
            "Update Game",
            1,
            29.99M,
            new DateOnly(2026, 1, 1));

        var updateResponse = await client.PutAsJsonAsync($"/games/{createdGame.Id}", updateGame);

        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        var getResponse = await client.GetAsync($"/games/{createdGame.Id}");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var updatedGame = await getResponse.Content.ReadFromJsonAsync<GameDetailsDto>();

        Assert.NotNull(updatedGame);
        Assert.Equal(updateGame.Name, updatedGame.Name);
    }

    [Fact]
    public async Task UpdateGame_InvalidGenre_ReturnsBadRequest()
    {
        using var factory = new GameStoreApiFactory();
        using var client = factory.CreateClient();

        var validGame = new CreateGameDto(
            "Valid Game",
            1,
            29.99M,
            new DateOnly(2026, 1, 1));

        var postResponse = await client.PostAsJsonAsync("/games", validGame);

        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);

        var createdGame = await postResponse.Content.ReadFromJsonAsync<GameDetailsDto>();
        Assert.NotNull(createdGame);

        var invalidUpdateGame = new UpdateGameDto(
            "Valid Game Updated",
            999999,
            29.99M,
            new DateOnly(2026, 1, 1));

        var updateResponse = await client.PutAsJsonAsync($"/games/{createdGame.Id}", invalidUpdateGame);

        Assert.Equal(HttpStatusCode.BadRequest, updateResponse.StatusCode);
    }


    [Fact]
    public async Task UpdateGame_DuplicateNameDifferentCasing_ReturnsConflict()
    {
        using var factory = new GameStoreApiFactory();
        using var client = factory.CreateClient();

        var gamesToCreate = new List<CreateGameDto>
        {
            new("Minecraft", 1, 59.99M, new DateOnly(2026, 1, 1)),
            new("Terraria", 1, 39.99M, new DateOnly(2026, 1, 1)),
        };

        var createdIds = new List<int>();

        foreach (var gameDto in gamesToCreate)
        {
            var postResponse = await client.PostAsJsonAsync("/games", gameDto);
            Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);

            var createdGame = await postResponse.Content.ReadFromJsonAsync<GameDetailsDto>();
            Assert.NotNull(createdGame);
            createdIds.Add(createdGame.Id);
        }

        var updateDto = new UpdateGameDto("minecraft", 1, 39.99M, new DateOnly(2026, 1, 1));
        var putResponse = await client.PutAsJsonAsync($"/games/{createdIds[1]}", updateDto);
        Assert.Equal(HttpStatusCode.Conflict, putResponse.StatusCode);
    }

    [Fact]
    public async Task UpdateGame_SameName_ReturnsNoContent()
    {
        using var factory = new GameStoreApiFactory();
        using var client = factory.CreateClient();

        var firstGame = new CreateGameDto(
            "Minecraft",
            1,
            29.99M,
            new DateOnly(2026, 1, 1));

        var postResponse = await client.PostAsJsonAsync("/games", firstGame);
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);

        var createdGame = await postResponse.Content.ReadFromJsonAsync<GameDetailsDto>();
        Assert.NotNull(createdGame);
        int gameId = createdGame.Id;

        var updatedGame = new UpdateGameDto(
            "Minecraft",
            1,
            19.99M,
            new DateOnly(2026, 1, 1));

        var putResponse = await client.PutAsJsonAsync($"/games/{gameId}", updatedGame);
        Assert.Equal(HttpStatusCode.NoContent, putResponse.StatusCode);

        var getResponse = await client.GetAsync($"/games/{gameId}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var updatedGamePrice = await getResponse.Content.ReadFromJsonAsync<GameDetailsDto>();
        Assert.NotNull(updatedGamePrice);
        Assert.Equal(19.99M, updatedGamePrice.Price);
    }

    [Fact]
    public async Task UpdateGame_DuplicateNameConflict_DoesNotModifyOriginalGame()
    {
        using var factory = new GameStoreApiFactory();
        using var client = factory.CreateClient();

        var gamesToCreate = new List<CreateGameDto>
        {
            new("Minecraft", 1, 59.99M, new DateOnly(2026, 1, 1)),
            new("Terraria", 1, 39.99M, new DateOnly(2026, 1, 1)),
        };

        var createdIds = new List<int>();

        foreach (var gameDto in gamesToCreate)
        {
            var postResponse = await client.PostAsJsonAsync("/games", gameDto);
            Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);

            var createdGame = await postResponse.Content.ReadFromJsonAsync<GameDetailsDto>();
            Assert.NotNull(createdGame);
            createdIds.Add(createdGame.Id);
        }

        var updateDto = new UpdateGameDto("minecraft", 1, 39.99M, new DateOnly(2026, 1, 1));
        var putResponse = await client.PutAsJsonAsync($"/games/{createdIds[1]}", updateDto);
        Assert.Equal(HttpStatusCode.Conflict, putResponse.StatusCode);

        var getResponse = await client.GetAsync($"/games/{createdIds[1]}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var updatedGameName = await getResponse.Content.ReadFromJsonAsync<GameDetailsDto>();
        Assert.NotNull(updatedGameName);
        Assert.Equal("Terraria", updatedGameName.Name);
    }

    [Fact]
    public async Task UpdateGame_InvalidGenre_DoesNotModifyExistingGame()
    {
        using var factory = new GameStoreApiFactory();
        using var client = factory.CreateClient();

        var firstGame = new CreateGameDto(
            "Minecraft",
            1,
            29.99M,
            new DateOnly(2026, 1, 1));

        var postResponse = await client.PostAsJsonAsync("/games", firstGame);
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);

        var createdGame = await postResponse.Content.ReadFromJsonAsync<GameDetailsDto>();
        Assert.NotNull(createdGame);
        int gameId = createdGame.Id;

        var updatedGame = new UpdateGameDto(
            "Minecraft",
            9999,
            29.99M,
            new DateOnly(2026, 1, 1));

        var putResponse = await client.PutAsJsonAsync($"/games/{gameId}", updatedGame);
        Assert.Equal(HttpStatusCode.BadRequest, putResponse.StatusCode);

        var getResponse = await client.GetAsync($"/games/{gameId}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var game = await getResponse.Content.ReadFromJsonAsync<GameDetailsDto>();
        Assert.NotNull(game);
        Assert.Equal(1, game.GenreId);
        Assert.Equal("Minecraft", game.Name);
    }

    [Fact]
    public async Task DeleteGame_ExistingId_ReturnsNoContent()
    {
        using var factory = new GameStoreApiFactory();
        using var client = factory.CreateClient();

        var gameToDelete = new CreateGameDto(
            "Game To Delete",
            1,
            29.99M,
            new DateOnly(2026, 1, 1));

        var response = await client.PostAsJsonAsync("/games", gameToDelete);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var createdGame = await response.Content.ReadFromJsonAsync<GameDetailsDto>();

        Assert.NotNull(createdGame);

        var deleteResponse = await client.DeleteAsync($"/games/{createdGame.Id}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteGame_NonExistingId_ReturnsNotFound()
    {
        using var factory = new GameStoreApiFactory();
        using var client = factory.CreateClient();

        var deleteResponse = await client.DeleteAsync("/games/999999");

        Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
    }


    [Fact]
    public async Task DeleteGame_DeletedGameCannotBeRetrieved()
    {
        using var factory = new GameStoreApiFactory();
        using var client = factory.CreateClient();

        var firstGame = new CreateGameDto(
            "Minecraft",
            1,
            29.99M,
            new DateOnly(2026, 1, 1));

        var postResponse = await client.PostAsJsonAsync("/games", firstGame);
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);

        var createdGame = await postResponse.Content.ReadFromJsonAsync<GameSummaryDto>();
        Assert.NotNull(createdGame);

        var deleteGame = await client.DeleteAsync($"/games/{createdGame.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteGame.StatusCode);

        var getResponse = await client.GetAsync($"/games/{createdGame.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

}
