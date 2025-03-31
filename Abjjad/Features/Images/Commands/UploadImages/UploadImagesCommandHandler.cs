using Abjjad.Services;
using MediatR;

namespace Abjjad.Features.Images.Commands.UploadImages;

public class UploadImagesCommandHandler(
    IServiceProvider serviceProvider,
    ILogger<UploadImagesCommandHandler> logger)
    : IRequestHandler<UploadImagesCommand, UploadImagesResponse>
{
    public async Task<UploadImagesResponse> Handle(UploadImagesCommand request, CancellationToken cancellationToken)
    {
        var uniqueIds = new List<string>();
        var errors = new List<string>();
        var backgroundService = serviceProvider.GetRequiredService<ImageProcessingBackgroundService>();

        foreach (var file in request.Files)
            try
            {
                var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream, cancellationToken);
                memoryStream.Position = 0;

                var uniqueId = Guid.NewGuid().ToString();
                backgroundService.EnqueueImage(memoryStream, file.FileName, uniqueId);
                uniqueIds.Add(uniqueId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing file: {FileName}", file.FileName);
                errors.Add($"Error processing {file.FileName}: {ex.Message}");
            }

        return new UploadImagesResponse(uniqueIds, errors);
    }
}