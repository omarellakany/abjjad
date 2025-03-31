using Abjjad.Models;

namespace Abjjad.Services;

public interface IImageProcessingService
{
    Task<ImageMetadata> ProcessImageAsync(Stream imageStream, string fileName, string id);
    Task<byte[]> GetResizedImageAsync(string id, string size);
    Task<ImageMetadata> GetImageMetadataAsync(string id);
}