using System.Net;
using System.Net.Http.Json;
using GameStore.API.Data;
using GameStore.API.DTOs;
using Microsoft.Extensions.DependencyInjection;

namespace GameStore.API.Tests;

public class GenresEndpointsTests
{
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

}
