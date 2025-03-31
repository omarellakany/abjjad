using Abjjad.Models;
using Abjjad.Services;
using MediatR;

namespace Abjjad.Features.Images.Queries.GetImageMetadata;

public class GetImageMetadataQueryHandler(IImageProcessingService imageProcessingService)
    : IRequestHandler<GetImageMetadataQuery, ImageMetadata>
{
    public async Task<ImageMetadata> Handle(GetImageMetadataQuery request, CancellationToken cancellationToken)
    {
        return await imageProcessingService.GetImageMetadataAsync(request.Id);
    }
}