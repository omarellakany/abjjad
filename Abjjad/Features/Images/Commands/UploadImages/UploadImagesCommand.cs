using MediatR;

namespace Abjjad.Features.Images.Commands.UploadImages;

public record UploadImagesCommand(IFormFileCollection Files) : IRequest<UploadImagesResponse>;

public record UploadImagesResponse(List<string> Ids, List<string> Errors);