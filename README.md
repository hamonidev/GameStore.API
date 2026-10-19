# GameStore API

A REST API built with ASP.NET Core and Entity Framework Core for managing a collection of games.

## Features

- Create, retrieve, update, and delete games
- View all available genres or retrieve a genre by ID
- Filter games by genre
- Search games by name
- Sort results by name, price, or release date
- Paginate large result sets
- Validate incoming requests and genre references
- Prevent duplicate game names with case-insensitive uniqueness checks
- Integration tests for API endpoints and database interactions
- Interactive API documentation with Swagger UI

## Technologies

- C#
- .NET 10
- ASP.NET Core Minimal APIs
- Entity Framework Core
- SQLite
- OpenAPI
- SwaggerUI
- xUnit
- Microsoft.AspNetCore.Mvc.Testing
- Microsoft.NET.Test.Sdk

## Getting Started

### Prerequisites

- .NET 10 SDK

### Run the API

Clone the repository:

```bash
git clone https://github.com/hamonidev/GameStore.API.git
cd GameStore.API
```

Run the application:

```bash
dotnet run --project GameStore.API/GameStore.API.csproj
```
The application uses SQLite for storage. When you run the API in development, database migrations are applied automatically and the default genres are seeded if none exist.

The console output will show the URL where the API is running.

## API Documentation

When running in the Development environment, the interactive API documentation is available through Swagger UI.

Replace the port with the one shown in your console and open this URL:

```bash
http://localhost:<port>/swagger
```

You will be able to explore all the available endpoints and send requests to the API.

## API Endpoints

| Method | Endpoint | Description |
|---|---|---|
| GET | `/games` | Get all games (supports filtering, searching, sorting, and pagination) |
| GET | `/games/{id}` | Get a game by ID |
| POST | `/games` | Create a new game |
| PUT | `/games/{id}` | Update an existing game |
| DELETE | `/games/{id}` | Delete a game |
| GET | `/genres` | Get all genres |
| GET | `/genres/{id}` | Get a genre by ID |

## Game Listing Parameters

The GET /games endpoint supports the following query parameters:

| Parameter |  Description |
|---|---|
| `genreId` | Filter games by genre ID |
| `search` | Search game names
| `sort` | Sort by name, price, price_desc, or releaseDate
| `page` | Select the page number |
| `pageSize | Choose the number of results per page |

# Example:

```bash
GET /games?search=wukong&sort=price&page=1&pageSize=10
```

## Running the Tests

Run all tests from the repository root:

```bash
dotnet test
```

The integration tests use a separate SQLite database to keep test data isolated from the development environment.

## About This Project

This is a personal learning project that I built while learning ASP.NET Core and backend development with C#.

The API started as a simple CRUD application and was gradually extended with filtering, searching, sorting, pagination, validation, database constraints, and integration tests as I learned new concepts.

## License

No license has been specified yet.
