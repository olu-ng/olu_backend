

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

# ✅ Install netcat (this solves your error)
RUN apt-get update && apt-get install -y netcat-traditional

COPY --from=build /app/publish .

# ✅ Copy the wait-for-it script and make it executable
COPY wait-for-it.sh ./wait-for-it.sh
RUN chmod +x ./wait-for-it.sh

# Run the application
# ENTRYPOINT ["dotnet", "OluBackendApp.dll"]
ENTRYPOINT ["./wait-for-it.sh", "sqlserver:1433", "--timeout=60", "--", "dotnet", "OluBackendApp.dll"]
