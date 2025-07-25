

# Use the ASP.NET 9 runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80

# Build stage with the .NET SDK
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
# Replace "OluBackendApp.csproj" with your actual .csproj filename if different
# COPY ["OluBackendApp.csproj", "./"]
COPY . .
RUN dotnet restore "OluBackendApp.csproj"

# Copy all source code and publish
COPY . .
RUN dotnet publish "OluBackendApp.csproj" -c Release -o /app/publish

# Final image: copy the published output
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

# Run the application
ENTRYPOINT ["dotnet", "OluBackendApp.dll"]

