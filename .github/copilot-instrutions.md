# Overview

This repository contains a .NET starter app for managing Contoso inventory. The application is split into three projects:

- `ContosoInventory/ContosoInventory.Server/` — ASP.NET Core Web API, EF Core, Identity, and authentication
- `ContosoInventory/ContosoInventory.Client/` — Blazor WebAssembly UI
- `ContosoInventory/ContosoInventory.Shared/` — DTOs shared between API and client

 # Contoso Inventory API - Coding Standards

 ## Naming Conventions
 - Use PascalCase for class names, public methods, and public properties.
 - Use camelCase for local variables and method parameters.
 - Prefix private fields with an underscore (e.g., _inventoryService).
 - Suffix interface names with the interface prefix "I" (e.g., IInventoryService).

 ## Architecture Patterns
 - Follow the repository pattern for all data access operations.
 - Use dependency injection for all service dependencies. Register services in Program.cs.
 - Separate business logic into service classes. Controllers should only handle HTTP concerns.
 - Use DTOs (Data Transfer Objects) for API request and response payloads. Never expose database entities directly.

 ## Error Handling
 - Use try-catch blocks for all external API calls and database operations.
 - Return appropriate HTTP status codes (200 for success, 400 for bad requests, 404 for not found, 500 for server errors).
 - Log all exceptions using ILogger<T> with structured logging.
 - Include meaningful error messages in API responses.

 ## Documentation
 - Include XML documentation comments on all public methods and classes.
 - Use inline comments only for complex business logic that is not self-explanatory.

 ## Testing
 - Write unit tests using xUnit and Moq.
 - Follow the Arrange-Act-Assert pattern in test methods.
 - Name test methods using the pattern: MethodName_Scenario_ExpectedResult.

## Build and run

From the repository root:

```bash
cd ContosoInventory

dotnet restore

dotnet build
```

To run the app:

```bash
dotnet run --project ContosoInventory.Server/ContosoInventory.Server.csproj
```

The app runs on `http://localhost:5240` by default and exposes Swagger at `http://localhost:5240/swagger` in development.

## When making changes

### Server changes

- Update controllers, services, and DTOs in the server project when adding or changing API behavior.
- Keep validation and authorization rules aligned with the existing controller/service pattern.
- If a new endpoint is added, update the corresponding client service and UI usage.

### Client changes

- Prefer modifying existing Blazor pages/components and existing service wrappers rather than introducing a separate data-access pattern.
- Keep UI work consistent with Bootstrap-based styling already used in the app.

### Shared models and DTOs

- If an API contract changes, update the related DTO in the `Shared` project and the corresponding server/client code together.
- Avoid creating duplicate models that overlap with the existing DTOs.


## Helpful references

- `README.md` — project overview, prerequisites, and run instructions
- `ContosoInventory/ContosoInventory.Server/Program.cs` — server startup and middleware configuration
- `ContosoInventory/ContosoInventory.Client/Program.cs` — client service registration
- `ContosoInventory/ContosoInventory.Server/Controllers/` — API endpoints
- `ContosoInventory/ContosoInventory.Client/Pages/` — page-level UI
- `ContosoInventory/ContosoInventory.Shared/DTOs/` — request/response contracts
