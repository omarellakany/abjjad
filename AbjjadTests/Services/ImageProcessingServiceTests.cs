using System.Text;
using Abjjad.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.PixelFormats;

namespace AbjjadTests.Services;

[TestFixture]
public class ImageProcessingServiceTests : IDisposable
{
  private readonly ImageProcessingService _service;
  private readonly string _testImagePath;
  private readonly string _testId;

  public ImageProcessingServiceTests()
  {
    _service = new ImageProcessingService();
    _testId = Guid.NewGuid().ToString();
    _testImagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Storage", _testId);
  }

  public void Dispose()
  {
    if (Directory.Exists(_testImagePath))
    {
      Directory.Delete(_testImagePath, true);
    }

    GC.SuppressFinalize(this);
  }

  private static async Task<Stream> CreateTestImageStreamAsync()
  {
    using var image = new Image<Rgba32>(100, 100);
    image[0, 0] = new Rgba32(255, 0, 0);

    // Add EXIF data
    var exifProfile = new ExifProfile();
    exifProfile.SetValue(ExifTag.DateTime, DateTime.UtcNow.ToString("yyyy:MM:dd HH:mm:ss"));
    exifProfile.SetValue(ExifTag.Make, "Test Camera");
    exifProfile.SetValue(ExifTag.Model, "Test Model");
    image.Metadata.ExifProfile = exifProfile;

    var stream = new MemoryStream();
    await image.SaveAsync(stream, new JpegEncoder());
    stream.Position = 0;
    return stream;
  }

  [Test]
  public async Task ProcessImageAsync_ValidImage_ReturnsMetadata()
  {
    await using var imageStream = await CreateTestImageStreamAsync();
    const string fileName = "test.jpg";

    var result = await _service.ProcessImageAsync(imageStream, fileName, _testId);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Id, Is.EqualTo(_testId));
    Assert.That(result.OriginalFileName, Is.EqualTo(fileName));
    Assert.That(result.UploadAt, Is.GreaterThan(DateTime.UtcNow.AddMinutes(-1)));
  }

  [Test]
  public async Task ProcessImageAsync_ValidImage_CreatesResizedVersions()
  {
    await using var imageStream = await CreateTestImageStreamAsync();
    var fileName = "test.jpg";

    await _service.ProcessImageAsync(imageStream, fileName, _testId);

    var phonePath = Path.Combine(_testImagePath, "phone.webp");
    var tabletPath = Path.Combine(_testImagePath, "tablet.webp");
    var desktopPath = Path.Combine(_testImagePath, "desktop.webp");

    Assert.That(File.Exists(phonePath), Is.True);
    Assert.That(File.Exists(tabletPath), Is.True);
    Assert.That(File.Exists(desktopPath), Is.True);
  }

  [Test]
  public async Task GetResizedImageAsync_ValidSize_ReturnsImageBytes()
  {
    await using var imageStream = await CreateTestImageStreamAsync();
    await _service.ProcessImageAsync(imageStream, "test.jpg", _testId);

    var result = await _service.GetResizedImageAsync(_testId, "phone");

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Length, Is.GreaterThan(0));
  }

  [Test]
  public async Task GetResizedImageAsync_InvalidSize_ThrowsArgumentException()
  {
    await using var imageStream = await CreateTestImageStreamAsync();
    await _service.ProcessImageAsync(imageStream, "test.jpg", _testId);

    var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
        await _service.GetResizedImageAsync(_testId, "invalid_size"));
    Assert.That(ex, Is.Not.Null);
  }

  [Test]
  public Task GetResizedImageAsync_NonExistentImage_ThrowsFileNotFoundException()
  {
    var ex = Assert.ThrowsAsync<FileNotFoundException>(async () =>
        await _service.GetResizedImageAsync("non_existent_id", "phone"));
    Assert.That(ex, Is.Not.Null);
    return Task.CompletedTask;
  }

  [Test]
  public async Task GetImageMetadataAsync_ValidId_ReturnsMetadata()
  {
    await using var imageStream = await CreateTestImageStreamAsync();
    const string fileName = "test.jpg";
    await _service.ProcessImageAsync(imageStream, fileName, _testId);

    var result = await _service.GetImageMetadataAsync(_testId);

    Assert.That(result, Is.Not.Null);
    Assert.That(result.Id, Is.EqualTo(_testId));
    Assert.That(result.OriginalFileName, Is.EqualTo(fileName));
  }

  [Test]
  public Task GetImageMetadataAsync_NonExistentId_ThrowsFileNotFoundException()
  {
    var ex = Assert.ThrowsAsync<FileNotFoundException>(async () =>
        await _service.GetImageMetadataAsync("non_existent_id"));
    Assert.That(ex, Is.Not.Null);

    return Task.CompletedTask;
  }
}