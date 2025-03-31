using Abjjad.Models;
using MediatR;

namespace Abjjad.Features.Images.Queries.GetResizedImage;

public record GetResizedImageQuery(string Id, ImageSize? Size) : IRequest<byte[]>;