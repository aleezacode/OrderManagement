# OrderManagement

A small Order Management microservice sample written in C# using ASP.NET Core.

This repository contains the service implementation, repositories, consumers, commands, handlers, and integration tests.

## Minimal quick start

From the repository root (macOS / zsh):

```bash
# restore, build and run the API
dotnet restore
dotnet build
dotnet run --project OrderManagement/OrderManagement.csproj

# run tests
dotnet test OrderManagementTests/OrderManagementTests.csproj
```

If you prefer Docker, the project includes a `Dockerfile.api` and `docker-compose.yml` for local development.

## Technologies used

- .NET 9 (net9.0)
- C# / ASP.NET Core Web API
- MongoDB (MongoDB.Driver)
- Apache Kafka (Confluent.Kafka)
- MediatR for in-process messaging
- FluentValidation for request validation
- Swashbuckle (Swagger) for API docs
- Docker / Docker Compose for containerized development

## Testing

- xUnit for unit and integration tests
- Testcontainers and Mongo2Go used in integration tests
- coverlet for code coverage collection

## Project structure (high level)

- `OrderManagement/` - main API project
- `Commands/`, `Consumers/`, `Handlers/`, `Repositories/`, `Models/`, `Controllers/` - core implementation
- `OrderManagementTests/` - tests and integration tests

## Notes

This README is intentionally minimal to get started. If you'd like, I can expand it with detailed run instructions, environment variables, Docker compose examples, or API documentation steps.
