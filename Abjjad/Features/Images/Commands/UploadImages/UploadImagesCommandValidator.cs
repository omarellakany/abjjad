using FluentValidation;

namespace Abjjad.Features.Images.Commands.UploadImages;

public class UploadImagesCommandValidator : AbstractValidator<UploadImagesCommand>
{
    private const int MaxFileSize = 2 * 1024 * 1024;
    private readonly string[] _allowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];

    public UploadImagesCommandValidator()
    {
        RuleFor(x => x.Files)
            .NotEmpty()
            .WithMessage("No files were uploaded");

        RuleForEach(x => x.Files)
            .Must(file => file.Length <= MaxFileSize)
            .WithMessage("File exceeds 2MB limit");

        RuleForEach(x => x.Files)
            .Must(file => _allowedExtensions.Contains(Path.GetExtension(file.FileName).ToLowerInvariant()))
            .WithMessage("File has invalid format. Only JPG, PNG, and WebP formats are allowed.");
    }
}