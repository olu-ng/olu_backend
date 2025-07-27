# Use ASP.NET 9 base image for runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80

# Build stage with .NET SDK
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files and restore
COPY ["OluBackendApp.csproj", "./"]
RUN dotnet restore "OluBackendApp.csproj"

# Copy rest of the source code
COPY . .
RUN dotnet publish "OluBackendApp.csproj" -c Release -o /app/publish

# Final image
FROM base AS final
WORKDIR /app

# ✅ Install netcat to use in wait-for-it.sh
RUN apt-get update && apt-get install -y netcat-traditional

# Copy published output
COPY --from=build /app/publish .

# ✅ Copy the wait script and make it executable
COPY wait-for-it.sh ./wait-for-it.sh
RUN chmod +x ./wait-for-it.sh

# ✅ DEBUG: Confirm the DLL exists (will print during build)
RUN ls -la && ls OluBackendApp.dll || echo "❌ OluBackendApp.dll NOT FOUND"

# Final entrypoint
ENTRYPOINT ["./wait-for-it.sh", "sqlserver:1433", "--timeout=60", "--", "dotnet", "OluBackendApp.dll"]
