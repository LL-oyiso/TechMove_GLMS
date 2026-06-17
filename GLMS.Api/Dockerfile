# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore first (better layer caching): copy only project files + global.json.
COPY ["global.json", "./"]
COPY ["GLMS.Api/GLMS.Api.csproj", "GLMS.Api/"]
COPY ["GLMS.Shared/GLMS.Shared.csproj", "GLMS.Shared/"]
RUN dotnet restore "GLMS.Api/GLMS.Api.csproj"

# Copy the rest of the source and publish.
COPY GLMS.Api/ GLMS.Api/
COPY GLMS.Shared/ GLMS.Shared/
RUN dotnet publish "GLMS.Api/GLMS.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "GLMS.Api.dll"]
