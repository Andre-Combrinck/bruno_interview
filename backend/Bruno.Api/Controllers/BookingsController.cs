using Bruno.Application.Bookings.Commands;
using Bruno.Application.Bookings.Queries;
using Bruno.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Bruno.Api.Controllers;

[ApiController]
[Route("api/bookings")]
public sealed class BookingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public BookingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] Guid? vehicleId,
        [FromQuery] Guid? customerId,
        [FromQuery] BookingStatus? status,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetBookingsQuery(vehicleId, customerId, status, from, to, page, pageSize),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetBookingByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBookingRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CreateBookingCommand(request.VehicleId, request.CustomerId, request.StartDate, request.EndDate),
            cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeBookingStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ChangeBookingStatusCommand(id, request.Status), cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteBookingCommand(id), cancellationToken);
        return NoContent();
    }
}

public sealed record CreateBookingRequest(Guid VehicleId, Guid CustomerId, DateOnly StartDate, DateOnly EndDate);
public sealed record ChangeBookingStatusRequest(BookingStatus Status);
