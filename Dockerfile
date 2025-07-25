# Use the ASP.NET 9 runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80

# Build stage with the .NET SDK
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY ["OluBackendApp.csproj", "./"]
RUN dotnet restore "OluBackendApp.csproj"

# Copy all source files (including config files) and publish
COPY . .
RUN dotnet publish "OluBackendApp.csproj" -c Release -o /app/publish

# Final image: copy the published output
FROM base AS final
WORKDIR /app

# Copy all files including appsettings*.json
COPY --from=build /app/publish .

# Copy the config files manually to ensure they're present
COPY ["appsettings.json", "appsettings.Production.json", "./"]

# Run the application
ENTRYPOINT ["dotnet", "OluBackendApp.dll"]
