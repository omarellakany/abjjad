using Abjjad.Models;
using MediatR;

namespace Abjjad.Features.Images.Queries.GetImageMetadata;

public record GetImageMetadataQuery(string Id) : IRequest<ImageMetadata>;