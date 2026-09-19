using Bruno.Application.Common.Dtos;
using Bruno.Application.Common.Interfaces;
using Bruno.Application.Common.Mappings;
using Bruno.Domain.Common;
using Bruno.Domain.Entities;
using Bruno.Domain.Enums;
using FluentValidation;
using MediatR;

namespace Bruno.Application.Bookings.Commands;

public sealed record CreateBookingCommand(
    Guid VehicleId,
    Guid CustomerId,
    DateOnly StartDate,
    DateOnly EndDate) : IRequest<BookingDto>;

public sealed class CreateBookingCommandValidator : AbstractValidator<CreateBookingCommand>
{
    public CreateBookingCommandValidator()
    {
        RuleFor(x => x.VehicleId).NotEmpty();
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.EndDate).GreaterThan(x => x.StartDate);
    }
}

public sealed class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand, BookingDto>
{
    private readonly IVehicleRepository _vehicles;
    private readonly ICustomerRepository _customers;
    private readonly IBookingRepository _bookings;
    private readonly IUnitOfWork _unitOfWork;

    public CreateBookingCommandHandler(
        IVehicleRepository vehicles,
        ICustomerRepository customers,
        IBookingRepository bookings,
        IUnitOfWork unitOfWork)
    {
        _vehicles = vehicles;
        _customers = customers;
        _bookings = bookings;
        _unitOfWork = unitOfWork;
    }

    public async Task<BookingDto> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
    {
        var vehicle = await _vehicles.GetByIdAsync(request.VehicleId, cancellationToken)
            ?? throw new DomainException("Vehicle not found.");

        _ = await _customers.GetByIdAsync(request.CustomerId, cancellationToken)
            ?? throw new DomainException("Customer not found.");

        var existing = await _bookings.GetActiveByVehicleAsync(request.VehicleId, cancellationToken);
        var booking = Booking.Create(vehicle, request.CustomerId, request.StartDate, request.EndDate, existing);

        await _bookings.AddAsync(booking, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return booking.ToDto();
    }
}

public sealed record ChangeBookingStatusCommand(Guid Id, BookingStatus Status) : IRequest<BookingDto>;

public sealed class ChangeBookingStatusCommandValidator : AbstractValidator<ChangeBookingStatusCommand>
{
    public ChangeBookingStatusCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Status).IsInEnum();
    }
}

public sealed class ChangeBookingStatusCommandHandler : IRequestHandler<ChangeBookingStatusCommand, BookingDto>
{
    private readonly IBookingRepository _bookings;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublisher _publisher;

    public ChangeBookingStatusCommandHandler(IBookingRepository bookings, IUnitOfWork unitOfWork, IPublisher publisher)
    {
        _bookings = bookings;
        _unitOfWork = unitOfWork;
        _publisher = publisher;
    }

    public async Task<BookingDto> Handle(ChangeBookingStatusCommand request, CancellationToken cancellationToken)
    {
        var booking = await _bookings.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new DomainException("Booking not found.");

        booking.ChangeStatus(request.Status);
        _bookings.Update(booking);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var domainEvent in booking.DomainEvents)
        {
            await _publisher.Publish(domainEvent, cancellationToken);
        }

        booking.ClearDomainEvents();
        return booking.ToDto();
    }
}

public sealed record DeleteBookingCommand(Guid Id) : IRequest;

public sealed class DeleteBookingCommandHandler : IRequestHandler<DeleteBookingCommand>
{
    private readonly IBookingRepository _bookings;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteBookingCommandHandler(IBookingRepository bookings, IUnitOfWork unitOfWork)
    {
        _bookings = bookings;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteBookingCommand request, CancellationToken cancellationToken)
    {
        var booking = await _bookings.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new DomainException("Booking not found.");

        booking.EnsureCanBeDeleted(DateOnly.FromDateTime(DateTime.UtcNow));
        _bookings.Remove(booking);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
