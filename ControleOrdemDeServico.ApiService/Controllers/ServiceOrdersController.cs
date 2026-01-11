using MediatR;
using Microsoft.AspNetCore.Mvc;
using OsService.Domain.Enums;
using OsService.Services.V1.ServiceOrders.GetServiceOrderById;
using OsService.Services.V1.ServiceOrders.OpenServiceOrder;
using OsService.Services.V1.ServiceOrders.SearchServiceOrders;

namespace OsService.ApiService.Controllers;

[ApiController]
[Route("v1/service-orders")]
public sealed class ServiceOrdersController(IMediator mediator) : ControllerBase
{
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
}
