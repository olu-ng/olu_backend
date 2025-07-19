# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src

# Copy csproj & restore
COPY ["OluBackendApp.csproj", "./"]
RUN dotnet restore

# Copy everything else & publish
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS final
WORKDIR /app

# Install ICU (for full globalization) and timezones
RUN apk add --no-cache icu-libs tzdata

# Tell .NET to use ICU rather than invariant-only mode
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Copy the published output
COPY --from=build /app/publish .

# Launch the app
ENTRYPOINT ["dotnet", "OluBackendApp.dll"]
