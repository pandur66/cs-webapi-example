# webapi-demo

ASP.NET Core Web API built with `.NET 10` and `SQLite`.

## Requirements

- `.NET SDK 10`
- `Docker` and `Docker Compose` optionally

## Run Locally

```bash
dotnet restore webapi-demo.slnx
dotnet run --project src/WebApiDemo.WebAPI/WebApiDemo.WebAPI.csproj
```

The API will be available at:

- `https://localhost:7xxx`
- `http://localhost:5xxx`

The exact ports are defined by ASP.NET Core at local runtime.

## Run With Docker

Build the image:

```bash
docker build -t webapi-demo .
```

Run the container:

```bash
docker run --rm -p 8080:8080 webapi-demo
```

The API will be available at:

- `http://localhost:8080`

## Run With Docker Compose

```bash
docker compose up --build
```

To run in the background:

```bash
docker compose up -d --build
```

To stop:

```bash
docker compose down
```

## Database

- The application uses `SQLite`
- The local database file is `database.db`
- In `docker-compose`, the `./database.db` file is mounted at `/app/database.db`

## CI Pipeline

The GitHub Actions pipeline:

- restores the solution in locked mode
- builds in `Release`
- runs tests with code coverage
- publishes the application artifacts
- builds the Docker image
