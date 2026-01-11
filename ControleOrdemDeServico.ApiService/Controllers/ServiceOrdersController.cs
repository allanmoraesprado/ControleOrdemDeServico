using MediatR;
using Microsoft.AspNetCore.Mvc;
using OsService.ApiService.V1.ServiceOrders.Dtos;
using OsService.Domain.Enums;
using OsService.Services.Exceptions;
using OsService.Services.V1.ServiceOrders.Attachments;
using OsService.Services.V1.ServiceOrders.ChangePrice;
using OsService.Services.V1.ServiceOrders.ChangeStatus;
using OsService.Services.V1.ServiceOrders.Dtos;
using OsService.Services.V1.ServiceOrders.GetServiceOrderById;
using OsService.Services.V1.ServiceOrders.OpenServiceOrder;
using OsService.Services.V1.ServiceOrders.SearchServiceOrders;

namespace OsService.ApiService.Controllers;

[ApiController]
[Route("v1/service-orders")]
public sealed class ServiceOrdersController(
    IMediator mediator,
    IWebHostEnvironment env) : ControllerBase
{

    private const long MaxFileSize = 5 * 1024 * 1024; // 5MB

    private string GetUploadsRoot()
        => Path.Combine(env.ContentRootPath, "data", "uploads");

    [HttpPost]
    public async Task<IActionResult> Open(
        [FromBody] OpenServiceOrderCommand cmd,
        CancellationToken ct)
    {
        var (id, number) = await mediator.Send(cmd, ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id, number });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var so = await mediator.Send(
            new GetServiceOrderByIdQuery(id), ct);

        if (so is null) return NotFound();
        return Ok(so);
    }

    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery] Guid? customerId,
        [FromQuery] ServiceOrderStatus? status,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken ct)
    {
        var items = await mediator.Send(
            new SearchServiceOrdersQuery(customerId, status, from, to), ct);

        return Ok(items);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> ChangeStatus(
        Guid id,
        [FromBody] ChangeServiceOrderStatusDto body,
        CancellationToken ct)
    {
        var dto = await mediator.Send(
            new ChangeServiceOrderStatusCommand(id, body.Status), ct);

        return Ok(dto);
    }

    [HttpPatch("{id:guid}/price")]
    public async Task<IActionResult> ChangePrice(
        Guid id,
        [FromBody] ChangeServiceOrderPriceDto body,
        CancellationToken ct)
    {
        var dto = await mediator.Send(
            new ChangeServiceOrderPriceCommand(id, body.Price), ct);

        return Ok(dto);
    }

    [HttpPost("{id:guid}/attachments/before")]
    [RequestSizeLimit(MaxFileSize)]
    public async Task<IActionResult> UploadBefore(
        Guid id,
        IFormFile file,
        CancellationToken ct)
    {
        return await UploadAttachmentInternal(id, ServiceOrderAttachmentType.Before, file, ct);
    }

    [HttpPost("{id:guid}/attachments/after")]
    [RequestSizeLimit(MaxFileSize)]
    public async Task<IActionResult> UploadAfter(
        Guid id,
        IFormFile file,
        CancellationToken ct)
    {
        return await UploadAttachmentInternal(id, ServiceOrderAttachmentType.After, file, ct);
    }

    private async Task<IActionResult> UploadAttachmentInternal(
        Guid serviceOrderId,
        ServiceOrderAttachmentType type,
        IFormFile file,
        CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            throw new ValidationException("File is required.");

        if (file.Length > MaxFileSize)
            throw new ValidationException("File exceeds the maximum allowed size of 5MB.");

        if (!IsSupportedContentType(file.ContentType))
            throw new ValidationException("Only JPG and PNG files are allowed.");

        if (!IsSupportedExtension(file.FileName))
            throw new ValidationException("Only JPG and PNG file extensions are allowed.");

        var uploadsRoot = GetUploadsRoot();
        Directory.CreateDirectory(uploadsRoot);

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var safeName = SanitizeFileName(Path.GetFileNameWithoutExtension(file.FileName)) + ext;

        var generatedName = $"{serviceOrderId}_{type}_{Guid.NewGuid():N}{ext}";
        var relativePath = Path.Combine("service-orders", serviceOrderId.ToString("N"), type.ToString().ToLowerInvariant());
        var fullFolder = Path.Combine(uploadsRoot, relativePath);
        Directory.CreateDirectory(fullFolder);

        var fullPath = Path.Combine(fullFolder, generatedName);
        var storagePath = Path.Combine(relativePath, generatedName).Replace('\\', '/');

        await using (var stream = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true))
        {
            await file.CopyToAsync(stream, ct);
        }

        var attachmentId = await mediator.Send(
            new UploadServiceOrderAttachmentCommand(
                serviceOrderId,
                type,
                safeName,
                file.ContentType,
                file.Length,
                storagePath),
            ct);

        return CreatedAtAction(
            nameof(GetAttachments),
            new { id = serviceOrderId },
            new UploadAttachmentResultDto(attachmentId));
    }

    [HttpGet("{id:guid}/attachments")]
    public async Task<IActionResult> GetAttachments(Guid id, CancellationToken ct)
    {
        var items = await mediator.Send(
            new GetServiceOrderAttachmentsQuery(id), ct);

        return Ok(items);
    }

    private static bool IsSupportedContentType(string? contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType)) return false;
        contentType = contentType.ToLowerInvariant();
        return contentType is "image/jpeg" or "image/png";
    }

    private static bool IsSupportedExtension(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext is ".jpg" or ".jpeg" or ".png";
    }

    private static string SanitizeFileName(string fileName)
    {
        var name = Path.GetFileNameWithoutExtension(fileName);
        var ext = Path.GetExtension(fileName);

        foreach (var c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');

        return name + ext;
    }
}
