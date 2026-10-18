using System.Net;
using System.Net.Http.Json;
using GameStore.API.DTOs;

namespace GameStore.API.Tests;

public class GamesQueryTests
{
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
