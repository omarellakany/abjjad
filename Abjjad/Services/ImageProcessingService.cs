using System.Text.Json;
using Abjjad.Models;
using ExifLib;
using Microsoft.OpenApi.Extensions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace Abjjad.Services;

public class ImageProcessingService : IImageProcessingService
{
    private readonly string _baseStoragePath;
    private readonly Dictionary<string, (int width, int height)> _sizeConfigurations;

    public ImageProcessingService()
    {
        _baseStoragePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Storage");
        Directory.CreateDirectory(_baseStoragePath);

        _sizeConfigurations = new Dictionary<string, (int width, int height)>
        {
            { ImageSize.Phone.GetDisplayName().ToLower(), (800, 600) },
            { ImageSize.Tablet.GetDisplayName().ToLower(), (1024, 768) },
            { ImageSize.Desktop.GetDisplayName().ToLower(), (1920, 1080) }
        };
    }

    public async Task<ImageMetadata> ProcessImageAsync(Stream imageStream, string fileName, string id)
    {
        var imageMetadata = new ImageMetadata
        {
            Id = id,
            OriginalFileName = fileName,
            UploadAt = DateTime.UtcNow
        };

        var imageDirectory = Path.Combine(_baseStoragePath, id);
        Directory.CreateDirectory(imageDirectory);

        try
        {
            var originalPath = Path.Combine(imageDirectory, "original.jpg");
            await using (var fileStream = File.Create(originalPath))
            {
                await imageStream.CopyToAsync(fileStream);
            }

            using (var reader = new ExifReader(originalPath))
            {
                if (reader.GetTagValue(ExifTags.Make, out string make))
                    imageMetadata.CameraMake = make;
                if (reader.GetTagValue(ExifTags.Model, out string model))
                    imageMetadata.CameraModel = model;
                if (reader.GetTagValue(ExifTags.DateTime, out DateTime dateTime))
                    imageMetadata.TakenAt = dateTime;
                if (reader.GetTagValue(ExifTags.GPSLatitude, out double[] latitude) &&
                    reader.GetTagValue(ExifTags.GPSLatitudeRef, out string latitudeRef))
                {
                    var lat = latitude[0] + latitude[1] / 60 + latitude[2] / 3600;
                    imageMetadata.Latitude =
                        latitudeRef.Equals("N", StringComparison.CurrentCultureIgnoreCase) ? lat : -lat;
                }

                if (reader.GetTagValue(ExifTags.GPSLongitude, out double[] longitude) &&
                    reader.GetTagValue(ExifTags.GPSLongitudeRef, out string longitudeRef))
                {
                    var lon = longitude[0] + longitude[1] / 60 + longitude[2] / 3600;
                    imageMetadata.Longitude =
                        longitudeRef.Equals("E", StringComparison.CurrentCultureIgnoreCase) ? lon : -lon;
                }
            }

            using (var image = await Image.LoadAsync(originalPath))
            {
                foreach (var size in _sizeConfigurations)
                {
                    var resizedImage = image.Clone(x => x
                        .Resize(size.Value.width, size.Value.height));

                    var outputPath = Path.Combine(imageDirectory, $"{size.Key}.webp");
                    await resizedImage.SaveAsync(outputPath, new WebpEncoder());
                }
            }

            var metadataPath = Path.Combine(imageDirectory, "metadata.json");
            await File.WriteAllTextAsync(metadataPath, JsonSerializer.Serialize(imageMetadata));

            return imageMetadata;
        }
        catch (Exception)
        {
            if (Directory.Exists(imageDirectory)) Directory.Delete(imageDirectory, true);
            throw;
        }
    }

    public async Task<byte[]> GetResizedImageAsync(string id, string size)
    {
        if (!_sizeConfigurations.ContainsKey(size))
            throw new ArgumentException("Invalid size specified");

        var imagePath = Path.Combine(_baseStoragePath, id, $"{size}.webp");
        if (!File.Exists(imagePath))
            throw new FileNotFoundException("Image not found");

        return await File.ReadAllBytesAsync(imagePath);
    }

    public async Task<ImageMetadata> GetImageMetadataAsync(string id)
    {
        var metadataPath = Path.Combine(_baseStoragePath, id, "metadata.json");
        if (!File.Exists(metadataPath))
            throw new FileNotFoundException("Metadata not found");

        var json = await File.ReadAllTextAsync(metadataPath);
        return JsonSerializer.Deserialize<ImageMetadata>(json)!;
    }
}