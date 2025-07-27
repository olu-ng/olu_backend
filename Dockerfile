# ----------------------------------------
# STAGE 1: Build the application
# ----------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project file and restore
COPY ["OluBackendApp.csproj", "./"]
RUN dotnet restore "OluBackendApp.csproj"

# Copy the rest of the files and publish
COPY . .
RUN dotnet publish "OluBackendApp.csproj" -c Release -o /app/publish

# ----------------------------------------
# STAGE 2: Run in runtime image
# ----------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Install netcat (required by wait-for-it.sh)
RUN apt-get update && apt-get install -y netcat-traditional

# Copy published output from build stage
COPY --from=build /app/publish .

# Copy wait-for-it.sh and make it executable
COPY wait-for-it.sh ./wait-for-it.sh
RUN chmod +x ./wait-for-it.sh

# Run the app after SQL Server is reachable
ENTRYPOINT ["./wait-for-it.sh", "sqlserver:1433", "--timeout=60", "--", "dotnet", "OluBackendApp.dll"]
