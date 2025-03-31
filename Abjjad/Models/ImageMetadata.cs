namespace Abjjad.Models;

public class ImageMetadata
{
    public string Id { get; init; } = string.Empty;
    public string? CameraMake { get; set; }
    public string? CameraModel { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public DateTime? TakenAt { get; set; }
    public string OriginalFileName { get; init; } = string.Empty;
    public DateTime UploadAt { get; init; }
}