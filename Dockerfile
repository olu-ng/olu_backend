# ---------------------------
# 1️⃣ Base runtime image
# ---------------------------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80

# ---------------------------
# 2️⃣ Build image with SDK
# ---------------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution/project files and restore dependencies
COPY *.csproj ./
RUN dotnet restore "OluBackendApp.csproj"

# Copy all remaining source files
COPY . .

# Publish the application to /app/publish
RUN dotnet publish "OluBackendApp.csproj" -c Release -o /app/publish

# ---------------------------
# 3️⃣ Final image
# ---------------------------
FROM base AS final
WORKDIR /app

# ✅ Install netcat (needed by wait-for-it.sh)
RUN apt-get update && apt-get install -y netcat-traditional

# Copy published app from build stage
COPY --from=build /app/publish .

# Copy wait-for-it script
COPY wait-for-it.sh ./wait-for-it.sh
RUN chmod +x ./wait-for-it.sh

# Startup entrypoint
ENTRYPOINT ["./wait-for-it.sh", "sqlserver:1433", "--timeout=60", "--", "dotnet", "OluBackendApp.dll"]
