using Abjjad.Features.Images.Commands.UploadImages;
using Abjjad.Features.Images.Queries.GetImageMetadata;
using Abjjad.Features.Images.Queries.GetResizedImage;
using Abjjad.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Abjjad.Controllers;

[ApiController]
[Route("api/images")]
public class ImageController(IMediator mediator) : ControllerBase
{
    [HttpPost("upload")]
    public async Task<ActionResult<List<string>>> UploadImages(IFormFileCollection files)
    {
        var command = new UploadImagesCommand(files);
        var result = await mediator.Send(command);

        return Ok(new
        {
            result.Ids, result.Errors
        });
    }

    [HttpGet("{id}/resized")]
    public async Task<ActionResult> GetResizedImage(string id, [FromQuery] ImageSize? size)
    {
        var query = new GetResizedImageQuery(id, size);
        var imageBytes = await mediator.Send(query);
        return File(imageBytes, "image/webp");
    }

    [HttpGet("{id}/metadata")]
    public async Task<ActionResult<ImageMetadata>> GetImageMetadata(string id)
    {
        var query = new GetImageMetadataQuery(id);
        var metadata = await mediator.Send(query);
        return Ok(metadata);
    }
}