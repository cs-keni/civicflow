FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/CivicFlow.API/CivicFlow.API.csproj", "src/CivicFlow.API/"]
COPY ["src/CivicFlow.Application/CivicFlow.Application.csproj", "src/CivicFlow.Application/"]
COPY ["src/CivicFlow.Domain/CivicFlow.Domain.csproj", "src/CivicFlow.Domain/"]
COPY ["src/CivicFlow.Infrastructure/CivicFlow.Infrastructure.csproj", "src/CivicFlow.Infrastructure/"]
COPY ["src/CivicFlow.Client/CivicFlow.Client.csproj", "src/CivicFlow.Client/"]

RUN dotnet restore "src/CivicFlow.API/CivicFlow.API.csproj"

COPY . .
WORKDIR "/src/src/CivicFlow.API"
RUN dotnet publish "CivicFlow.API.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CivicFlow.API.dll"]
