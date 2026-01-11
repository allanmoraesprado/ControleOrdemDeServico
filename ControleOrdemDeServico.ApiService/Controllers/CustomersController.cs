using MediatR;
using Microsoft.AspNetCore.Mvc;
using OsService.Services.V1.Customers.CreateCustomer;
using OsService.Services.V1.Customers.GetCustomerById;
using OsService.Services.V1.Customers.SearchCustomer;

namespace OsService.ApiService.Controllers;

[ApiController]
[Route("v1/customers")]
public sealed class CustomersController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCustomerCommand cmd, CancellationToken ct)
    {
        var id = await mediator.Send(cmd, ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var customer = await mediator.Send(new GetCustomerByIdQuery(id), ct);
        if (customer is null) return NotFound();
        return Ok(customer);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string? phone, [FromQuery] string? document, CancellationToken ct)
    {
        var customer = await mediator.Send(
            new GetCustomerByPhoneOrDocumentQuery(phone, document), ct);

        if (customer is null) return NotFound();
        return Ok(customer);
    }
}