using Abjjad.Services;
using MediatR;
using Microsoft.OpenApi.Extensions;

namespace Abjjad.Features.Images.Queries.GetResizedImage;

public class GetResizedImageQueryHandler(IImageProcessingService imageProcessingService)
    : IRequestHandler<GetResizedImageQuery, byte[]>
{
    public async Task<byte[]> Handle(GetResizedImageQuery request, CancellationToken cancellationToken)
    {
        if (request.Size == null)
            throw new ArgumentException("Size must be provided");

        return await imageProcessingService.GetResizedImageAsync(request.Id, request.Size.GetDisplayName().ToLower());
    }
}