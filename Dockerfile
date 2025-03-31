# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy csproj and restore dependencies
COPY ["Abjjad/Abjjad.csproj", "Abjjad/"]
RUN dotnet restore "Abjjad/Abjjad.csproj"

# Copy the rest of the code
COPY . .

# Build and publish
RUN dotnet publish "Abjjad/Abjjad.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Create a non-root user
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

# Create directory for image storage
RUN mkdir -p /app/Storage && chown -R appuser /app/Storage

# Copy published files from build stage
COPY --from=build /app/publish .

# Expose the port the app will run on
EXPOSE 8080

# Set the entry point
ENTRYPOINT ["dotnet", "Abjjad.dll"] 