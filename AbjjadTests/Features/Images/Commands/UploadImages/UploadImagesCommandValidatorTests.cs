using Abjjad.Features.Images.Commands.UploadImages;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Http;

namespace AbjjadTests.Features.Images.Commands.UploadImages;

[TestFixture]
public class UploadImagesCommandValidatorTests
{
  private UploadImagesCommandValidator? _validator;

  [OneTimeSetUp]
  public void Setup()
  {
    _validator = new UploadImagesCommandValidator();
  }

  [Test]
  public void Validate_EmptyFiles_ShouldHaveValidationError()
  {
    var command = new UploadImagesCommand(new FormFileCollection());

    var result = _validator.TestValidate(command);

    result.ShouldHaveValidationErrorFor(x => x.Files)
        .WithErrorMessage("No files were uploaded");
  }

  [Test]
  public void Validate_ValidFiles_ShouldNotHaveValidationError()
  {
    var files = new FormFileCollection
        {
            CreateFormFile("test.jpg", 1024 * 1024), // 1MB
            CreateFormFile("test.png", 1024 * 1024),
            CreateFormFile("test.webp", 1024 * 1024)
        };
    var command = new UploadImagesCommand(files);

    var result = _validator.TestValidate(command);

    result.ShouldNotHaveAnyValidationErrors();
  }

  [Test]
  public void Validate_FileTooLarge_ShouldHaveValidationError()
  {
    var files = new FormFileCollection
        {
            CreateFormFile("test.jpg", 3 * 1024 * 1024) // 3MB
        };
    var command = new UploadImagesCommand(files);

    var result = _validator.TestValidate(command);

    result.ShouldHaveValidationErrorFor(x => x.Files)
        .WithErrorMessage("File exceeds 2MB limit");
  }

  [Test]
  public void Validate_InvalidFileExtension_ShouldHaveValidationError()
  {
    var files = new FormFileCollection
        {
            CreateFormFile("test.gif", 1024 * 1024)
        };
    var command = new UploadImagesCommand(files);

    var result = _validator.TestValidate(command);

    result.ShouldHaveValidationErrorFor(x => x.Files)
        .WithErrorMessage("File has invalid format. Only JPG, PNG, and WebP formats are allowed.");
  }

  [Test]
  public void Validate_MixedValidAndInvalidFiles_ShouldHaveValidationErrors()
  {
    var files = new FormFileCollection
        {
            CreateFormFile("test.jpg", 1024 * 1024), // Valid
            CreateFormFile("test.gif", 1024 * 1024), // Invalid extension
            CreateFormFile("test.png", 3 * 1024 * 1024) // Too large
        };
    var command = new UploadImagesCommand(files);

    var result = _validator.TestValidate(command);

    result.ShouldHaveValidationErrorFor(x => x.Files)
        .WithErrorMessage("File has invalid format. Only JPG, PNG, and WebP formats are allowed.");
    result.ShouldHaveValidationErrorFor(x => x.Files)
        .WithErrorMessage("File exceeds 2MB limit");
  }

  protected virtual IFormFile CreateFormFile(string fileName, long length)
  {
    var stream = new MemoryStream(new byte[length]);
    return new FormFile(stream, 0, length, "Files", fileName)
    {
      Headers = new HeaderDictionary(),
      ContentType = "image/jpeg"
    };
  }
}