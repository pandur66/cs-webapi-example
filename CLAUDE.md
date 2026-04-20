# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Restore dependencies
dotnet restore webapi-demo.slnx

# Build
dotnet build webapi-demo.slnx --configuration Release

# Run locally
dotnet run --project src/WebApiDemo.WebAPI/WebApiDemo.WebAPI.csproj

# Run all tests
dotnet test webapi-demo.slnx

# Run a single test (by name filter)
dotnet test webapi-demo.slnx --filter "FullyQualifiedName~TestMethodName"

# Docker
docker compose up --build

# Add a new EF Core migration
dotnet ef migrations add <Name> \
  --project src/WebApiDemo.Infrastructure \
  --startup-project src/WebApiDemo.WebAPI

# Apply migrations
dotnet ef database update \
  --project src/WebApiDemo.Infrastructure \
  --startup-project src/WebApiDemo.WebAPI

# Regenerate package lock files (run after changing dependencies)
dotnet restore --force-evaluate webapi-demo.slnx
```

## Architecture

Clean Architecture ASP.NET Core 10 Web API with SQLite via Entity Framework Core.

```
src/
  WebApiDemo.Domain/          → Entities (ToDoItem), repository interfaces (IToDoRepository)
  WebApiDemo.Application/     → Service interfaces (IToDoService), service implementations, DTOs
  WebApiDemo.Infrastructure/  → EF Core DbContext, repository implementations, Migrations
  WebApiDemo.WebAPI/          → Controllers, Program.cs (DI wiring)
tests/
  WebApiDemo.Tests/
    Application/              → ToDoService integration tests (Application + Infrastructure)
    WebAPI/                   → TodoController unit tests
```

**Dependency rule:** Domain has no external references. Application references Domain only. Infrastructure references Domain. WebAPI references Application and Infrastructure (only for DI wiring in `Program.cs`).

**Key patterns:**
- Controllers map between DTOs and domain entities; all DB access flows through the service → repository chain
- All service and controller methods are async
- `ToDoService.PatchAsync` fetches via repository, applies the patch doc, then calls `UpdateAsync` — `JsonPatchDocument<T>` does not leak into the repository layer
- JSON Patch (`PATCH /api/todo/{id}`) uses Newtonsoft.Json; the rest of the app uses System.Text.Json
- OpenAPI UI served via Scalar at `/scalar/v1`
- Logging via `ILogger<T>` injected into `ToDoService`; write-operations log at `Information`, not-found cases at `Warning`

**Testing:** xUnit 3 with Coverlet for coverage. Tests live in `tests/WebApiDemo.Tests/`. `ToDoServiceTests` constructs `ToDoRepository` + `ToDoService` directly against an in-memory EF Core database. Code coverage config is in `coverlet.runsettings`.

**Lock files:** All projects have `RestorePackagesWithLockFile` enabled. The CI restore runs with `--locked-mode`. Commit `packages.lock.json` changes whenever dependencies change.

**Code style:** Enforced via `.editorconfig` — file-scoped namespaces, 4-space indent, UTF-8.
