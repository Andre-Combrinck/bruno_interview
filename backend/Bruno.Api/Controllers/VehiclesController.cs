using Bruno.Application.Vehicles.Commands;
using Bruno.Application.Vehicles.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Bruno.Api.Controllers;

[ApiController]
[Route("api/vehicles")]
public sealed class VehiclesController : ControllerBase
{
    private readonly IMediator _mediator;

    public VehiclesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] string? search,
        [FromQuery] bool includeDeleted = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetVehiclesQuery(search, includeDeleted, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetVehicleByIdQuery(id, includeDeleted), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateVehicleRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CreateVehicleCommand(request.RegistrationNumber, request.Make, request.Model, request.Year, request.DailyRate),
            cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateVehicleRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new UpdateVehicleCommand(id, request.RegistrationNumber, request.Make, request.Model, request.Year, request.DailyRate),
            cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new SoftDeleteVehicleCommand(id), cancellationToken);
        return NoContent();
    }
}

public sealed record CreateVehicleRequest(string RegistrationNumber, string Make, string Model, int Year, decimal DailyRate);
public sealed record UpdateVehicleRequest(string RegistrationNumber, string Make, string Model, int Year, decimal DailyRate);
