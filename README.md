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

## Technologies

- C#
- ASP.NET Core Minimal APIs
- Entity Framework Core
- SQLite
- xUnit
- Microsoft.AspNetCore.Mvc.Testing
- Microsoft.NET.Test.Sdk

## Getting Started

### Prerequisites

- .NET 10 SDK
- Visual Studio, Visual Studio Code, or another C# editor

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

Once the application is running, the console output will show the URL where the API is available.

## Running the Tests

Run all tests from the repository root:

```bash
dotnet test
```

The integration tests use a separate SQLite database to keep test data isolated from the development environment.

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


## About This Project

This is a personal learning project that I built while learning ASP.NET Core and backend development with C#.

The API started as a simple CRUD application and was gradually extended with filtering, searching, sorting, pagination, validation, database constraints, and integration tests as I learned new concepts.

## License

No license has been specified yet.
