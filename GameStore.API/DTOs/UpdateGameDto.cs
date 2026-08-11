namespace GameStore.API.DTOs;

public record UpdateGameDto(
    string Name,
    string Genre,
    decimal Price,
    DateOnly ReleaseDate
);
