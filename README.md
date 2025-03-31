# Abjjad Demo

A .NET 8 Web API for processing and managing images with the following features:

- Upload images (JPG, PNG, WebP) with size limit of 2MB
- Automatic conversion to WebP format
- Resize images to three common dimensions (phone, tablet, desktop)
- Extract and store EXIF metadata
- Download resized images
- Retrieve image metadata

## Architecture & Features

This project implements several modern architectural patterns and best practices:

- **CQRS (Command Query Responsibility Segregation)**: Using MediatR for handling commands and queries
- **Unit Testing**: Comprehensive test coverage using xUnit and Moq
- **Background Jobs**: Image processing tasks run in the background using IHostedService
- **Containerization**: Docker support for easy deployment and scaling
- **Error Handling**: Global error handling middleware with detailed error responses
- **Input Validation**: FluentValidation for request validation
- **Clean Architecture**: Separation of concerns with Services, Models, and Features

## Prerequisites

- .NET 8.0 SDK or later
- Docker (optional, for containerized deployment)
- An IDE (Visual Studio, VS Code, or Rider)

## Running the Application

### Using dotnet CLI

1. Clone the repository
2. Navigate to the project directory:
   ```bash
   cd Abjjad
   ```
3. Build the project:
   ```bash
   dotnet build
   ```
4. Run the application:
   ```bash
   dotnet run
   ```

### Using Docker

1. Build the Docker image:

   ```bash
   docker build -t abjjad .
   ```

2. Run the container:
   ```bash
   docker run -d \
     -p 7001:8080 \
     -e ASPNETCORE_URLS=http://+:8080 \
     -e ASPNETCORE_ENVIRONMENT=Development \
     -v $(pwd)/Storage:/app/Storage \
     abjjad
   ```

The API will be available at:

- http://localhost:7001 (HTTP)
- Swagger UI: http://localhost:7001/swagger

## API Endpoints

### Upload Image

- **POST** `/api/image/upload`
- Accepts multipart/form-data with a file field
- Returns image metadata including a unique ID

### Get Resized Image

- **GET** `/api/image/{id}/resized?size={size}`
- Parameters:
  - `id`: The unique identifier of the image
  - `size`: One of "phone", "tablet", or "desktop"
- Returns the resized image in WebP format

### Get Image Metadata

- **GET** `/api/image/{id}/metadata`
- Parameter:
  - `id`: The unique identifier of the image
- Returns the image metadata in JSON format

## Storage

All images and metadata are stored in the file system under the `Storage` directory in the application's base directory.
Each image gets its own subdirectory named with its unique ID, containing:

- Original image
- Resized versions in WebP format
- Metadata JSON file

## Error Handling

The API includes comprehensive error handling for:

- Invalid file formats
- File size limits
- Missing files
- Invalid size parameters
- Processing errors

## Security

- CORS is enabled for development purposes
- File size limits are enforced
- File type validation is performed
- Container runs as non-root user for enhanced security
- Secure file handling practices are implemented

## Running Tests

To run the unit tests:

```bash
dotnet test
```

The test project includes comprehensive tests for:

- Image processing service
- Input validation
- Error handling
- File operations
- Background job processing
