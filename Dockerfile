FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["webapi-demo.csproj", "./"]
COPY ["NuGet.Config", "./"]
RUN dotnet restore "webapi-demo.csproj" --configfile NuGet.Config

COPY . .
RUN dotnet publish "webapi-demo.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
COPY --from=build /src/database.db ./database.db

ENTRYPOINT ["dotnet", "webapi-demo.dll"]
