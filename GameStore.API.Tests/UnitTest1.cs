using System.Net;
using System.Net.Http.Json;
using GameStore.API.Data;
using GameStore.API.DTOs;
using GameStore.API.Models;
using Microsoft.Extensions.DependencyInjection;

namespace GameStore.API.Tests;

public class UnitTest1
{
    [Fact]
    public async Task GetGame_NonExistingId_ReturnsNotFound()
    {
        using var factory = new GameStoreApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/games/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

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
    public async Task GetGames_SearchTerm_ReturnsMatchingGames()
    {
        using var factory = new GameStoreApiFactory();
        using var client = factory.CreateClient();

        var gamesToCreate = new List<CreateGameDto>
        {
            new("Dragon Quest", 2, 49.99M, new DateOnly(2026, 1, 1)),
            new("Space Racing", 3, 39.99M, new DateOnly(2026, 1, 1)),
        };

        foreach (var gameDto in gamesToCreate)
        {
            var postResponse = await client.PostAsJsonAsync("/games", gameDto);
            Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
        }

        var getResponse = await client.GetAsync($"/games?search=Dragon");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var games = await getResponse.Content.ReadFromJsonAsync<List<GameSummaryDto>>();

        Assert.NotNull(games);
        Assert.Contains(games, g => g.Name == "Dragon Quest");
        Assert.DoesNotContain(games, g => g.Name == "Space Racing");
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
    public async Task GetGames_SortByPrice_ReturnsAscendingOrder()
    {
        using var factory = new GameStoreApiFactory();
        using var client = factory.CreateClient();

        var expensiveGame = new CreateGameDto(
            "Expensive Game",
            1,
            59.99M,
            new DateOnly(2026, 1, 1));

        var cheapGame = new CreateGameDto(
            "Cheap Game",
            1,
            9.99M,
            new DateOnly(2026, 1, 1));

        var midRangeGame = new CreateGameDto(
            "Midrange Game",
            1,
            29.99M,
            new DateOnly(2026, 1, 1));

        var responseExpensive = await client.PostAsJsonAsync("/games", expensiveGame);
        Assert.Equal(HttpStatusCode.Created, responseExpensive.StatusCode);

        var responseCheap = await client.PostAsJsonAsync("/games", cheapGame);
        Assert.Equal(HttpStatusCode.Created, responseCheap.StatusCode);

        var responseMid = await client.PostAsJsonAsync("/games", midRangeGame);
        Assert.Equal(HttpStatusCode.Created, responseMid.StatusCode);

        var getResponse = await client.GetAsync($"/games?sort=price");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var games = await getResponse.Content.ReadFromJsonAsync<List<GameSummaryDto>>();

        Assert.NotNull(games);
        Assert.Equal("Cheap Game", games[0].Name);
        Assert.Equal("Midrange Game", games[1].Name);
        Assert.Equal("Expensive Game", games[2].Name);
    }

    [Fact]
    public async Task GetGames_SortByPriceDescending_ReturnsCorrectOrder()
    {
        using var factory = new GameStoreApiFactory();
        using var client = factory.CreateClient();

        var expensiveGame = new CreateGameDto(
            "Expensive Game",
            1,
            59.99M,
            new DateOnly(2026, 1, 1));

        var cheapGame = new CreateGameDto(
            "Cheap Game",
            1,
            9.99M,
            new DateOnly(2026, 1, 1));

        var midRangeGame = new CreateGameDto(
            "Midrange Game",
            1,
            29.99M,
            new DateOnly(2026, 1, 1));

        var responseExpensive = await client.PostAsJsonAsync("/games", expensiveGame);
        Assert.Equal(HttpStatusCode.Created, responseExpensive.StatusCode);

        var responseCheap = await client.PostAsJsonAsync("/games", cheapGame);
        Assert.Equal(HttpStatusCode.Created, responseCheap.StatusCode);

        var responseMid = await client.PostAsJsonAsync("/games", midRangeGame);
        Assert.Equal(HttpStatusCode.Created, responseMid.StatusCode);

        var getResponse = await client.GetAsync($"/games?sort=price_desc");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var games = await getResponse.Content.ReadFromJsonAsync<List<GameSummaryDto>>();

        Assert.NotNull(games);
        Assert.Equal("Expensive Game", games[0].Name);
        Assert.Equal("Midrange Game", games[1].Name);
        Assert.Equal("Cheap Game", games[2].Name);
    }

    [Fact]
    public async Task GetGames_PageSize_ReturnsRequestedNumberOfGames()
    {
        using var factory = new GameStoreApiFactory();
        using var client = factory.CreateClient();

        var gamesToCreate = new List<CreateGameDto>
        {
            new("First Game", 1, 59.99M, new DateOnly(2026, 1, 1)),
            new("Second Game", 1, 49.99M, new DateOnly(2026, 1, 1)),
            new("Third Game", 1, 39.99M, new DateOnly(2026, 1, 1)),
        };

        foreach (var gameDto in gamesToCreate)
        {
            var postResponse = await client.PostAsJsonAsync("/games", gameDto);
            Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
        }

        var getResponse = await client.GetAsync("/games?page=1&pageSize=2");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var gamesInPage = await getResponse.Content.ReadFromJsonAsync<List<GameSummaryDto>>();

        Assert.NotNull(gamesInPage);
        Assert.Equal(2, gamesInPage.Count);
    }

    [Fact]
    public async Task GetGames_DifferentPages_ReturnDifferentGames()
    {
        using var factory = new GameStoreApiFactory();
        using var client = factory.CreateClient();

        var gamesToCreate = new List<CreateGameDto>
        {
            new("First Game", 1, 59.99M, new DateOnly(2026, 1, 1)),
            new("Second Game", 1, 49.99M, new DateOnly(2026, 1, 1)),
            new("Third Game", 1, 39.99M, new DateOnly(2026, 1, 1)),
        };

        foreach (var gameDto in gamesToCreate)
        {
            var postResponse = await client.PostAsJsonAsync("/games", gameDto);
            Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
        }

        var firstGetResponse = await client.GetAsync("/games?page=1&pageSize=2");
        Assert.Equal(HttpStatusCode.OK, firstGetResponse.StatusCode);

        var secondGetResponse = await client.GetAsync("/games?page=2&pageSize=2");
        Assert.Equal(HttpStatusCode.OK, secondGetResponse.StatusCode);

        var gamesInFirstPage = await firstGetResponse.Content.ReadFromJsonAsync<List<GameSummaryDto>>();
        var gamesInSecondPage = await secondGetResponse.Content.ReadFromJsonAsync<List<GameSummaryDto>>();

        Assert.NotNull(gamesInFirstPage);
        Assert.NotNull(gamesInSecondPage);
        Assert.Equal(2, gamesInFirstPage.Count);
        var gameOnSecondPage = Assert.Single(gamesInSecondPage);
        Assert.DoesNotContain(gamesInFirstPage, game => game.Id == gameOnSecondPage.Id);
    }

    [Fact]
    public async Task GetGames_PageZero_NormalizesToFirstPage()
    {
        using var factory = new GameStoreApiFactory();
        using var client = factory.CreateClient();

        var gamesToCreate = new List<CreateGameDto>
        {
            new("First Game", 1, 59.99M, new DateOnly(2026, 1, 1)),
            new("Second Game", 1, 49.99M, new DateOnly(2026, 1, 1)),
            new("Third Game", 1, 39.99M, new DateOnly(2026, 1, 1)),
        };

        foreach (var gameDto in gamesToCreate)
        {
            var postResponse = await client.PostAsJsonAsync("/games", gameDto);
            Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
        }

        var firstGetResponse = await client.GetAsync("/games?page=0&pageSize=2");
        Assert.Equal(HttpStatusCode.OK, firstGetResponse.StatusCode);

        var secondGetResponse = await client.GetAsync("/games?page=1&pageSize=2");
        Assert.Equal(HttpStatusCode.OK, secondGetResponse.StatusCode);

        var gamesInFirstPage = await firstGetResponse.Content.ReadFromJsonAsync<List<GameSummaryDto>>();
        var gamesInSecondPage = await secondGetResponse.Content.ReadFromJsonAsync<List<GameSummaryDto>>();

        Assert.NotNull(gamesInFirstPage);
        Assert.NotNull(gamesInSecondPage);

        var firstPageIds = gamesInFirstPage.Select(g => g.Id).ToList();
        var secondPageIds = gamesInSecondPage.Select(g => g.Id).ToList();

        Assert.Equal(secondPageIds, firstPageIds);
    }

    [Fact]
    public async Task GetGames_OversizedPageSize_UsesMaximum()
    {
        using var factory = new GameStoreApiFactory();
        using var client = factory.CreateClient();

        const int totalGamesToCreate = 31;

        for (int i = 1; i <= totalGamesToCreate; i++)
        {
            var newGame = new CreateGameDto(
                $"Test Game {i}",
                1,
                12.99M,
                new DateOnly(2026, 1, 1));

            var postResponse = await client.PostAsJsonAsync("/games", newGame);

            Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
        }

        var getResponse = await client.GetAsync("/games?page=1&pageSize=100");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var games = await getResponse.Content.ReadFromJsonAsync<List<GameSummaryDto>>();
        Assert.NotNull(games);

        Assert.Equal(30, games.Count);
    }

    [Fact]
    public async Task GetGames_ZeroPageSize_UsesMinimum()
    {
        using var factory = new GameStoreApiFactory();
        using var client = factory.CreateClient();

        var gamesToCreate = new List<CreateGameDto>
        {
            new("First Game", 1, 59.99M, new DateOnly(2026, 1, 1)),
            new("Second Game", 1, 49.99M, new DateOnly(2026, 1, 1)),
        };

        foreach (var gameDto in gamesToCreate)
        {
            var postResponse = await client.PostAsJsonAsync("/games", gameDto);
            Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
        }

        var getResponse = await client.GetAsync("/games?page=1&pageSize=0");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var games = await getResponse.Content.ReadFromJsonAsync<List<GameSummaryDto>>();
        Assert.NotNull(games);

        Assert.Single(games);
    }

    [Fact]
    public async Task GetGames_NoPageSizeProvided_UsesDefault()
    {
        using var factory = new GameStoreApiFactory();
        using var client = factory.CreateClient();

        var gamesToCreate = new List<CreateGameDto>
        {
            new("First Game", 1, 59.99M, new DateOnly(2026, 1, 1)),
            new("Second Game", 1, 49.99M, new DateOnly(2026, 1, 1)),
        };

        foreach (var gameDto in gamesToCreate)
        {
            var postResponse = await client.PostAsJsonAsync("/games", gameDto);
            Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
        }

        var getResponse = await client.GetAsync("/games?page=1");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var games = await getResponse.Content.ReadFromJsonAsync<List<GameSummaryDto>>();
        Assert.NotNull(games);

        Assert.Equal(2, games.Count);
    }

    [Fact]
    public async Task GetGames_InvalidSortParameter_ReturnsDefaultOrder()
    {
        using var factory = new GameStoreApiFactory();
        using var client = factory.CreateClient();

        var gamesToCreate = new List<CreateGameDto>
        {
            new("First Game", 1, 59.99M, new DateOnly(2026, 1, 1)),
            new("Second Game", 1, 49.99M, new DateOnly(2026, 1, 1)),
            new("Third Game", 1, 39.99M, new DateOnly(2026, 1, 1)),
        };

        foreach (var gameDto in gamesToCreate)
        {
            var postResponse = await client.PostAsJsonAsync("/games", gameDto);
            Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
        }

        var getResponse = await client.GetAsync("/games?sort=invalid");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var games = await getResponse.Content.ReadFromJsonAsync<List<GameSummaryDto>>();
        Assert.NotNull(games);
        Assert.Equal(3, games.Count);

        for (int i = 0; i < games.Count - 1; i++)
        {
            Assert.True(games[i].Id < games[i + 1].Id);
        }
    }

    [Fact]
    public async Task GetGames_SortByReleaseDate_ReturnsAscendingOrder()
    {
        using var factory = new GameStoreApiFactory();
        using var client = factory.CreateClient();

        var gamesToCreate = new List<CreateGameDto>
        {
            new("First Game", 1, 59.99M, new DateOnly(2026, 9, 5)),
            new("Second Game", 1, 49.99M, new DateOnly(2024, 3, 4)),
            new("Third Game", 1, 39.99M, new DateOnly(2025, 12, 2)),
        };

        foreach (var gameDto in gamesToCreate)
        {
            var postResponse = await client.PostAsJsonAsync("/games", gameDto);
            Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
        }

        var getResponse = await client.GetAsync("/games?sort=releasedate");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var games = await getResponse.Content.ReadFromJsonAsync<List<GameSummaryDto>>();
        Assert.NotNull(games);
        Assert.Equal(3, games.Count);

        for (int i = 0; i < games.Count - 1; i++)
        {
            Assert.True(games[i].ReleaseDate <= games[i + 1].ReleaseDate);
        }
    }

    [Fact]
    public async Task GetGames_GenreFilterAndPriceSort_ReturnsFilteredAndSortedGames()
    {
        using var factory = new GameStoreApiFactory();
        using var client = factory.CreateClient();

        var gamesToCreate = new List<CreateGameDto>
        {
            new("First Game", 1, 59.99M, new DateOnly(2026, 1, 1)),
            new("Second Game", 1, 49.99M, new DateOnly(2026, 1, 1)),
            new("Third Game", 2, 39.99M, new DateOnly(2026, 1, 1)),
        };

        foreach (var gameDto in gamesToCreate)
        {
            var postResponse = await client.PostAsJsonAsync("/games", gameDto);
            Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
        }

        var getResponse = await client.GetAsync("/games?genreId=1&sort=price");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var games = await getResponse.Content.ReadFromJsonAsync<List<GameSummaryDto>>();

        Assert.NotNull(games);
        Assert.Equal(2, games.Count);

        Assert.All(games, game => Assert.Equal("Fighting", game.Genre));

        Assert.Equal(49.99M, games[0].Price);
        Assert.Equal(59.99M, games[1].Price);
    }

    [Fact]
    public async Task GetGames_SearchTermAndPriceSort_ReturnsMatchingGamesInOrder()
    {
        using var factory = new GameStoreApiFactory();
        using var client = factory.CreateClient();

        var gamesToCreate = new List<CreateGameDto>
        {
            new("First Adventure", 1, 59.99M, new DateOnly(2026, 1, 1)),
            new("Second Adventure", 1, 49.99M, new DateOnly(2026, 1, 1)),
            new("Third Race", 2, 39.99M, new DateOnly(2026, 1, 1)),
        };

        foreach (var gameDto in gamesToCreate)
        {
            var postResponse = await client.PostAsJsonAsync("/games", gameDto);
            Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
        }

        var getResponse = await client.GetAsync("/games?search=Adventure&sort=price");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var games = await getResponse.Content.ReadFromJsonAsync<List<GameSummaryDto>>();

        Assert.NotNull(games);
        Assert.Equal(2, games.Count);

        Assert.All(games, game => Assert.Contains("Adventure", game.Name));

        Assert.Equal(49.99M, games[0].Price);
        Assert.Equal(59.99M, games[1].Price);
    }

    [Fact]
    public async Task GetGames_SearchTerm_IsCaseInsensitive()
    {
        using var factory = new GameStoreApiFactory();
        using var client = factory.CreateClient();

        var gamesToCreate = new List<CreateGameDto>
        {
            new("First Adventure", 1, 59.99M, new DateOnly(2026, 1, 1)),
            new("Second Adventure", 1, 49.99M, new DateOnly(2026, 1, 1)),
            new("Third Race", 2, 39.99M, new DateOnly(2026, 1, 1)),
        };

        foreach (var gameDto in gamesToCreate)
        {
            var postResponse = await client.PostAsJsonAsync("/games", gameDto);
            Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
        }

        var getResponse = await client.GetAsync("/games?search=adventure");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var games = await getResponse.Content.ReadFromJsonAsync<List<GameSummaryDto>>();

        Assert.NotNull(games);
        Assert.Equal(2, games.Count);
    }

    [Fact]
    public async Task GetGames_SearchTerm_NoMatches_ReturnsEmptyList()
    {
        using var factory = new GameStoreApiFactory();
        using var client = factory.CreateClient();

        var gamesToCreate = new List<CreateGameDto>
        {
            new("First Game", 1, 59.99M, new DateOnly(2026, 1, 1)),
            new("Second Game", 1, 49.99M, new DateOnly(2026, 1, 1)),
        };

        foreach (var gameDto in gamesToCreate)
        {
            var postResponse = await client.PostAsJsonAsync("/games", gameDto);
            Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
        }

        var getResponse = await client.GetAsync("/games?search=NonexistentGame");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var games = await getResponse.Content.ReadFromJsonAsync<List<GameSummaryDto>>();

        Assert.NotNull(games);
        Assert.Empty(games);
    }

    [Fact]
    public async Task GetGames_SortByPriceAndPageSize_ReturnsCheapestGames()
    {
        using var factory = new GameStoreApiFactory();
        using var client = factory.CreateClient();

        var gamesToCreate = new List<CreateGameDto>
        {
            new("First Game", 1, 59.99M, new DateOnly(2026, 1, 1)),
            new("Second Game", 1, 29.99M, new DateOnly(2026, 1, 1)),
            new("Third Game", 1, 49.99M, new DateOnly(2026, 1, 1)),
            new("Fourth Game", 1, 39.99M, new DateOnly(2026, 1, 1)),
        };

        foreach (var gameDto in gamesToCreate)
        {
            var postResponse = await client.PostAsJsonAsync("/games", gameDto);
            Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
        }

        var getResponse = await client.GetAsync("/games?sort=price&page=1&pageSize=2");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var games = await getResponse.Content.ReadFromJsonAsync<List<GameSummaryDto>>();

        Assert.NotNull(games);
        Assert.Equal(2, games.Count);
        Assert.Equal(29.99M, games[0].Price);
        Assert.Equal(39.99M, games[1].Price);
    }

    [Fact]
    public async Task GetGames_SortByPriceAndSecondPage_ReturnsNextCheapestGames()
    {
        using var factory = new GameStoreApiFactory();
        using var client = factory.CreateClient();

        var gamesToCreate = new List<CreateGameDto>
        {
            new("First Game", 1, 59.99M, new DateOnly(2026, 1, 1)),
            new("Second Game", 1, 29.99M, new DateOnly(2026, 1, 1)),
            new("Third Game", 1, 49.99M, new DateOnly(2026, 1, 1)),
            new("Fourth Game", 1, 39.99M, new DateOnly(2026, 1, 1)),
        };

        foreach (var gameDto in gamesToCreate)
        {
            var postResponse = await client.PostAsJsonAsync("/games", gameDto);
            Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
        }

        var getResponse = await client.GetAsync("/games?sort=price&page=2&pageSize=2");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var games = await getResponse.Content.ReadFromJsonAsync<List<GameSummaryDto>>();

        Assert.NotNull(games);
        Assert.Equal(2, games.Count);
        Assert.Equal(49.99M, games[0].Price);
        Assert.Equal(59.99M, games[1].Price);
    }

    [Fact]
    public async Task GetGames_PageBeyondAvailableResults_ReturnsEmptyList()
    {
        using var factory = new GameStoreApiFactory();
        using var client = factory.CreateClient();

        var gamesToCreate = new List<CreateGameDto>
        {
            new("First Game", 1, 59.99M, new DateOnly(2026, 1, 1)),
            new("Second Game", 1, 29.99M, new DateOnly(2026, 1, 1)),
            new("Third Game", 1, 49.99M, new DateOnly(2026, 1, 1)),
        };

        foreach (var gameDto in gamesToCreate)
        {
            var postResponse = await client.PostAsJsonAsync("/games", gameDto);
            Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
        }

        var getResponse = await client.GetAsync("/games?page=3&pageSize=2");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var games = await getResponse.Content.ReadFromJsonAsync<List<GameSummaryDto>>();

        Assert.NotNull(games);
        Assert.Empty(games);
    }

    [Fact]
    public async Task GetGames_PageAboveMaximum_UsesMaximumPage()
    {
        using var factory = new GameStoreApiFactory();
        using var client = factory.CreateClient();

        var gamesToCreate = new List<CreateGameDto>
        {
            new("First Game", 1, 59.99M, new DateOnly(2026, 1, 1)),
            new("Second Game", 1, 29.99M, new DateOnly(2026, 1, 1)),
            new("Third Game", 1, 49.99M, new DateOnly(2026, 1, 1)),
        };

        foreach (var gameDto in gamesToCreate)
        {
            var postResponse = await client.PostAsJsonAsync("/games", gameDto);
            Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
        }

        var getResponse = await client.GetAsync("/games?page=100&pageSize=2");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var games = await getResponse.Content.ReadFromJsonAsync<List<GameSummaryDto>>();

        Assert.NotNull(games);
        Assert.Empty(games);
    }

    [Fact]
    public async Task GetGames_NegativePage_UsesFirstPage()
    {
        using var factory = new GameStoreApiFactory();
        using var client = factory.CreateClient();

        var gamesToCreate = new List<CreateGameDto>
        {
            new("First Game", 1, 59.99M, new DateOnly(2026, 1, 1)),
            new("Second Game", 1, 29.99M, new DateOnly(2026, 1, 1)),
            new("Third Game", 1, 49.99M, new DateOnly(2026, 1, 1)),
        };

        foreach (var gameDto in gamesToCreate)
        {
            var postResponse = await client.PostAsJsonAsync("/games", gameDto);
            Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
        }

        var getResponse = await client.GetAsync("/games?page=-1&pageSize=2");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var games = await getResponse.Content.ReadFromJsonAsync<List<GameSummaryDto>>();

        Assert.NotNull(games);
        Assert.Equal(2, games.Count);

        var secondGetResponse = await client.GetAsync("/games?page=1&pageSize=2");

        Assert.Equal(HttpStatusCode.OK, secondGetResponse.StatusCode);

        var gamesInTheOtherPage = await secondGetResponse.Content.ReadFromJsonAsync<List<GameSummaryDto>>();
        Assert.NotNull(gamesInTheOtherPage);

        var firstPageIds = games.Select(g => g.Id).ToList();
        var secondPageIds = gamesInTheOtherPage.Select(g => g.Id).ToList();

        Assert.Equal(secondPageIds, firstPageIds);
    }

    [Fact]
    public async Task GetGames_NonExistingGenreId_ReturnsEmptyList()
    {
        using var factory = new GameStoreApiFactory();
        using var client = factory.CreateClient();

        var gamesToCreate = new List<CreateGameDto>
        {
            new("First Game", 1, 59.99M, new DateOnly(2026, 1, 1)),
            new("Second Game", 1, 29.99M, new DateOnly(2026, 1, 1)),
        };

        foreach (var gameDto in gamesToCreate)
        {
            var postResponse = await client.PostAsJsonAsync("/games", gameDto);
            Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
        }

        var getResponse = await client.GetAsync("/games?genreId=9999");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var games = await getResponse.Content.ReadFromJsonAsync<List<GameSummaryDto>>();

        Assert.NotNull(games);
        Assert.Empty(games);
    }

    [Fact]
    public async Task GetGames_GenreFilterAndSearchNoMatches_ReturnsEmptyList()
    {
        using var factory = new GameStoreApiFactory();
        using var client = factory.CreateClient();

        var gamesToCreate = new List<CreateGameDto>
        {
            new("Dragon Killer", 1, 59.99M, new DateOnly(2026, 1, 1)),
            new("Hydra Hunter", 1, 29.99M, new DateOnly(2026, 1, 1)),
        };

        foreach (var gameDto in gamesToCreate)
        {
            var postResponse = await client.PostAsJsonAsync("/games", gameDto);
            Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
        }

        var getResponse = await client.GetAsync("/games?genreId=1&search=Racing");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var games = await getResponse.Content.ReadFromJsonAsync<List<GameSummaryDto>>();

        Assert.NotNull(games);
        Assert.Empty(games);
    }

    [Fact]
    public async Task GetGames_GenreFilterAndSearch_ReturnsMatchingGames()
    {
        using var factory = new GameStoreApiFactory();
        using var client = factory.CreateClient();

        var gamesToCreate = new List<CreateGameDto>
        {
            new("Dragon Hunter", 1, 59.99M, new DateOnly(2026, 1, 1)),
            new("Hydra Hunter", 1, 29.99M, new DateOnly(2026, 1, 1)),
            new("Dragon Hunter II", 2, 49.99M, new DateOnly(2026, 1, 1)),
        };

        foreach (var gameDto in gamesToCreate)
        {
            var postResponse = await client.PostAsJsonAsync("/games", gameDto);
            Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
        }

        var getResponse = await client.GetAsync("/games?genreId=1&search=Dragon");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var games = await getResponse.Content.ReadFromJsonAsync<List<GameSummaryDto>>();

        Assert.NotNull(games);
        Assert.Single(games);
        Assert.Contains(games, g => g.Name == "Dragon Hunter");
        Assert.Equal("Fighting", games[0].Genre);
    }

    [Fact]
    public async Task GetGames_SortByPrice_HandlesEqualPrices()
    {
        using var factory = new GameStoreApiFactory();
        using var client = factory.CreateClient();

        var gamesToCreate = new List<CreateGameDto>
        {
            new("First Game", 1, 59.99M, new DateOnly(2026, 1, 1)),
            new("Second Game", 1, 59.99M, new DateOnly(2026, 1, 1)),
            new("Third Game", 1, 49.99M, new DateOnly(2026, 1, 1)),
        };

        foreach (var gameDto in gamesToCreate)
        {
            var postResponse = await client.PostAsJsonAsync("/games", gameDto);
            Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
        }

        var getResponse = await client.GetAsync("/games?sort=price");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var games = await getResponse.Content.ReadFromJsonAsync<List<GameSummaryDto>>();

        Assert.NotNull(games);
        Assert.Equal(3, games.Count);
        Assert.Equal(49.99M, games[0].Price);
        Assert.Equal(59.99M, games[1].Price);
        Assert.Equal(59.99M, games[2].Price);
    }

    [Fact]
    public async Task GetGames_SortByPriceDescending_HandlesEqualPrices()
    {
        using var factory = new GameStoreApiFactory();
        using var client = factory.CreateClient();

        var gamesToCreate = new List<CreateGameDto>
        {
            new("First Game", 1, 59.99M, new DateOnly(2026, 1, 1)),
            new("Second Game", 1, 59.99M, new DateOnly(2026, 1, 1)),
            new("Third Game", 1, 49.99M, new DateOnly(2026, 1, 1)),
        };

        foreach (var gameDto in gamesToCreate)
        {
            var postResponse = await client.PostAsJsonAsync("/games", gameDto);
            Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
        }

        var getResponse = await client.GetAsync("/games?sort=price_desc");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var games = await getResponse.Content.ReadFromJsonAsync<List<GameSummaryDto>>();

        Assert.NotNull(games);
        Assert.Equal(3, games.Count);
        Assert.Equal(59.99M, games[0].Price);
        Assert.Equal(59.99M, games[1].Price);
        Assert.Equal(49.99M, games[2].Price);
    }

    [Fact]
    public async Task GetGames_EmptySearch_ReturnsAllGames()
    {
        using var factory = new GameStoreApiFactory();
        using var client = factory.CreateClient();

        var gamesToCreate = new List<CreateGameDto>
        {
            new("First Game", 1, 59.99M, new DateOnly(2026, 1, 1)),
            new("Second Game", 1, 59.99M, new DateOnly(2026, 1, 1)),
            new("Third Game", 1, 49.99M, new DateOnly(2026, 1, 1)),
        };

        foreach (var gameDto in gamesToCreate)
        {
            var postResponse = await client.PostAsJsonAsync("/games", gameDto);
            Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
        }

        var getResponse = await client.GetAsync("/games?search=");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var games = await getResponse.Content.ReadFromJsonAsync<List<GameSummaryDto>>();

        Assert.NotNull(games);
        Assert.Equal(3, games.Count);
    }

    [Fact]
    public async Task GetGames_WhitespaceSearch_ReturnsAllGames()
    {
        using var factory = new GameStoreApiFactory();
        using var client = factory.CreateClient();

        var gamesToCreate = new List<CreateGameDto>
        {
            new("First Game", 1, 59.99M, new DateOnly(2026, 1, 1)),
            new("Second Game", 1, 39.99M, new DateOnly(2026, 1, 1)),
            new("Third Game", 1, 49.99M, new DateOnly(2026, 1, 1)),
        };

        foreach (var gameDto in gamesToCreate)
        {
            var postResponse = await client.PostAsJsonAsync("/games", gameDto);
            Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
        }

        var getResponse = await client.GetAsync("/games?search=%20%20");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var games = await getResponse.Content.ReadFromJsonAsync<List<GameSummaryDto>>();

        Assert.NotNull(games);
        Assert.Equal(3, games.Count);
    }

    [Fact]
    public async Task GetGames_SearchWithUrlEncodedCharacters_ReturnsMatchingGames()
    {
        using var factory = new GameStoreApiFactory();
        using var client = factory.CreateClient();

        var gamesToCreate = new List<CreateGameDto>
        {
            new("Dragon Hunter", 1, 59.99M, new DateOnly(2026, 1, 1)),
            new("Dragon Quest", 1, 39.99M, new DateOnly(2026, 1, 1)),
            new("Space Adventure", 1, 49.99M, new DateOnly(2026, 1, 1)),
        };

        foreach (var gameDto in gamesToCreate)
        {
            var postResponse = await client.PostAsJsonAsync("/games", gameDto);
            Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
        }

        var getResponse = await client.GetAsync("/games?search=Dragon%20Hunter");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var games = await getResponse.Content.ReadFromJsonAsync<List<GameSummaryDto>>();

        Assert.NotNull(games);
        Assert.Single(games);
        Assert.Equal("Dragon Hunter", games[0].Name);
    }

    [Fact]
    public async Task GetGames_PartialSearch_ReturnsMatchingGames()
    {
        using var factory = new GameStoreApiFactory();
        using var client = factory.CreateClient();

        var gamesToCreate = new List<CreateGameDto>
        {
            new("Dragon Hunter", 1, 59.99M, new DateOnly(2026, 1, 1)),
            new("Dragon Quest", 1, 39.99M, new DateOnly(2026, 1, 1)),
            new("Space Adventure", 1, 49.99M, new DateOnly(2026, 1, 1)),
        };

        foreach (var gameDto in gamesToCreate)
        {
            var postResponse = await client.PostAsJsonAsync("/games", gameDto);
            Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
        }

        var getResponse = await client.GetAsync("/games?search=Dragon");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var games = await getResponse.Content.ReadFromJsonAsync<List<GameSummaryDto>>();

        Assert.NotNull(games);
        Assert.Equal(2, games.Count);
        Assert.Contains(games, game => game.Name == "Dragon Hunter");
        Assert.Contains(games, game => game.Name == "Dragon Quest");
    }

    [Fact]
    public async Task GetGames_SearchWithPercentCharacter_ReturnsMatchingGames()
    {
        using var factory = new GameStoreApiFactory();
        using var client = factory.CreateClient();

        var gamesToCreate = new List<CreateGameDto>
        {
            new("100% Complete", 1, 59.99M, new DateOnly(2026, 1, 1)),
            new("100 Percent", 1, 39.99M, new DateOnly(2026, 1, 1)),
            new("Dragon Hunter", 1, 49.99M, new DateOnly(2026, 1, 1)),
        };

        foreach (var gameDto in gamesToCreate)
        {
            var postResponse = await client.PostAsJsonAsync("/games", gameDto);
            Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
        }

        var getResponse = await client.GetAsync("/games?search=%25");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var games = await getResponse.Content.ReadFromJsonAsync<List<GameSummaryDto>>();
        Assert.NotNull(games);

        Assert.Single(games);
        Assert.Contains(games, g => g.Name == "100% Complete");
    }

    [Fact]
    public async Task GetGames_SearchWithUnderscoreCharacter_ReturnsMatchingGames()
    {
        using var factory = new GameStoreApiFactory();
        using var client = factory.CreateClient();

        var gamesToCreate = new List<CreateGameDto>
        {
            new("Game_One", 1, 59.99M, new DateOnly(2026, 1, 1)),
            new("GameXOne", 1, 39.99M, new DateOnly(2026, 1, 1)),
            new("Dragon Hunter", 1, 49.99M, new DateOnly(2026, 1, 1)),
        };

        foreach (var gameDto in gamesToCreate)
        {
            var postResponse = await client.PostAsJsonAsync("/games", gameDto);
            Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
        }

        var getResponse = await client.GetAsync("/games?search=%5F");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var games = await getResponse.Content.ReadFromJsonAsync<List<GameSummaryDto>>();
        Assert.NotNull(games);

        Assert.Single(games);
        Assert.Contains(games, g => g.Name == "Game_One");
    }

    [Fact]
    public async Task GetGames_SearchWithExclamationMark_ReturnsMatchingGames()
    {
        using var factory = new GameStoreApiFactory();
        using var client = factory.CreateClient();

        var gamesToCreate = new List<CreateGameDto>
        {
            new("Wow! Game", 1, 59.99M, new DateOnly(2026, 1, 1)),
            new("Wow Game", 1, 39.99M, new DateOnly(2026, 1, 1)),
            new("Dragon Hunter", 1, 49.99M, new DateOnly(2026, 1, 1)),
        };

        foreach (var gameDto in gamesToCreate)
        {
            var postResponse = await client.PostAsJsonAsync("/games", gameDto);
            Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
        }

        var getResponse = await client.GetAsync("/games?search=%21");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var games = await getResponse.Content.ReadFromJsonAsync<List<GameSummaryDto>>();
        Assert.NotNull(games);

        Assert.Single(games);
        Assert.Contains(games, g => g.Name == "Wow! Game");
    }

    [Fact]
    public async Task GetGenres_ReturnsGenres()
    {
        using var factory = new GameStoreApiFactory();
        using var client = factory.CreateClient();

        var getResponse = await client.GetAsync("/genres");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var genres = await getResponse.Content.ReadFromJsonAsync<List<GenreDto>>();

        Assert.NotNull(genres);
        Assert.Equal(4, genres.Count);

        var genreNames = genres.Select(g => g.Name).ToList();

        Assert.Contains("Fighting", genreNames);
        Assert.Contains("RPG", genreNames);
        Assert.Contains("Racing", genreNames);
        Assert.Contains("Sports", genreNames);
    }

    [Fact]
    public async Task GetGenre_NonExistingId_ReturnsNotFound()
    {
        using var factory = new GameStoreApiFactory();
        using var client = factory.CreateClient();

        var getResponse = await client.GetAsync("/genres/9999");

        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task GetGenre_ExistingId_ReturnsGenre()
    {
        using var factory = new GameStoreApiFactory();
        using var client = factory.CreateClient();

        var getResponse = await client.GetAsync("/genres/1");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var genres = await getResponse.Content.ReadFromJsonAsync<GenreDto>();

        Assert.NotNull(genres);
        Assert.Equal("Fighting", genres.Name);
    }

    [Fact]
    public async Task GetGenre_ExistingId_ReturnsCorrectGenreDetails()
    {
        using var factory = new GameStoreApiFactory();
        using var client = factory.CreateClient();

        var getResponse = await client.GetAsync("/genres/2");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var genre = await getResponse.Content.ReadFromJsonAsync<GenreDto>();

        Assert.NotNull(genre);
        Assert.Equal(2, genre.Id);
        Assert.Equal("RPG", genre.Name);
    }

    [Fact]
    public async Task GetGenres_EmptyDatabase_ReturnsEmptyList()
    {
        using var factory = new GameStoreApiFactory();
        using var client = factory.CreateClient();

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<GameStoreContext>();

            dbContext.Genres.RemoveRange(dbContext.Genres);
            await dbContext.SaveChangesAsync();
        }

        var getResponse = await client.GetAsync("/genres");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var genres = await getResponse.Content.ReadFromJsonAsync<List<GenreDto>>();

        Assert.NotNull(genres);
        Assert.Empty(genres);
    }

    [Fact]
    public async Task GetGenre_EmptyDatabase_ReturnsNotFound()
    {
        using var factory = new GameStoreApiFactory();
        using var client = factory.CreateClient();

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<GameStoreContext>();

            dbContext.Genres.RemoveRange(dbContext.Genres);
            await dbContext.SaveChangesAsync();
        }

        var getResponse = await client.GetAsync("/genres/1");

        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
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
    public async Task GetGames_SearchIsCaseInsensitive_ReturnsMatchingGames()
    {
        using var factory = new GameStoreApiFactory();
        using var client = factory.CreateClient();

        var gamesToCreate = new List<CreateGameDto>
        {
            new("Minecraft", 1, 59.99M, new DateOnly(2026, 1, 1)),
            new("Terraria", 1, 39.99M, new DateOnly(2026, 1, 1)),
        };

        foreach (var gameDto in gamesToCreate)
        {
            var postResponse = await client.PostAsJsonAsync("/games", gameDto);
            Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
        }

        var getResponse = await client.GetAsync("/games?search=mINECRAFT");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var games = await getResponse.Content.ReadFromJsonAsync<List<GameSummaryDto>>();
        Assert.NotNull(games);
        Assert.Single(games);
        Assert.Equal("Minecraft", games[0].Name);
    }

    [Fact]
    public async Task GetGames_SearchNoMatches_ReturnsEmptyList()
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

        var getResponse = await client.GetAsync("/games?search=Terraria");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var games = await getResponse.Content.ReadFromJsonAsync<List<GameSummaryDto>>();
        Assert.NotNull(games);
        Assert.Empty(games);
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
    public async Task GetGames_FilterSortAndPaginate_ReturnsExpectedResults()
    {
        using var factory = new GameStoreApiFactory();
        using var client = factory.CreateClient();

        var gamesToCreate = new List<CreateGameDto>
        {
            new("Nubecraft", 1, 30.00M, new DateOnly(2026, 1, 1)),
            new("Terraria", 1, 20.00M, new DateOnly(2026, 1, 1)),
            new("Stardew Valley", 1, 10.00M, new DateOnly(2026, 1, 1)),
        };

        foreach (var gameDto in gamesToCreate)
        {
            var postResponse = await client.PostAsJsonAsync("/games", gameDto);
            Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
        }

        var getResponse = await client.GetAsync("/games?genreId=1&sort=price&page=1&pageSize=2");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var games = await getResponse.Content.ReadFromJsonAsync<List<GameSummaryDto>>();

        Assert.NotNull(games);
        Assert.Equal(2, games.Count);
        Assert.Equal(10.00M, games[0].Price);
        Assert.Equal(20.00M, games[1].Price);
        Assert.Equal("Stardew Valley", games[0].Name);
        Assert.Equal("Terraria", games[1].Name);
    }
}

