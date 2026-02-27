using System.IO;
using LeadFlow.Application.Common.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace LeadFlow.API.Endpoints;

public static class BlobEndpoints
{
    public static void MapBlobEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/blobs").WithTags("Blobs");

        group.MapPost("/upload", async (IFormFile file, [FromServices] IBlobStorageService blobService, CancellationToken ct) =>
        {
            if (file == null || file.Length == 0)
                return Results.BadRequest("No file uploaded.");

            using var stream = file.OpenReadStream();
            var blobName = await blobService.UploadAsync(stream, file.ContentType, file.FileName, ct);
            return Results.Ok(new { Name = blobName });
        })
        .DisableAntiforgery(); // Usually needed for minimal API file uploads

        group.MapGet("/{blobName}", async (string blobName, [FromServices] IBlobStorageService blobService, CancellationToken ct) =>
        {
            try
            {
                var (content, contentType) = await blobService.DownloadAsync(blobName, ct);
                return Results.File(content, contentType, blobName);
            }
            catch (Azure.RequestFailedException ex) when (ex.Status == 404)
            {
                return Results.NotFound();
            }
        });

        group.MapGet("/", async ([FromServices] IBlobStorageService blobService, CancellationToken ct) =>
        {
            var blobs = await blobService.ListAsync(ct);
            return Results.Ok(blobs);
        });

        group.MapDelete("/{blobName}", async (string blobName, [FromServices] IBlobStorageService blobService, CancellationToken ct) =>
        {
            await blobService.DeleteAsync(blobName, ct);
            return Results.NoContent();
        });
    }
}
