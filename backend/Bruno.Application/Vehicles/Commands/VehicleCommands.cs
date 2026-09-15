using Bruno.Application.Common.Dtos;
using Bruno.Application.Common.Interfaces;
using Bruno.Application.Common.Mappings;
using Bruno.Domain.Common;
using Bruno.Domain.Entities;
using FluentValidation;
using MediatR;

namespace Bruno.Application.Vehicles.Commands;

public sealed record CreateVehicleCommand(
    string RegistrationNumber,
    string Make,
    string Model,
    int Year,
    decimal DailyRate) : IRequest<VehicleDto>;

public sealed class CreateVehicleCommandValidator : AbstractValidator<CreateVehicleCommand>
{
    public CreateVehicleCommandValidator()
    {
        RuleFor(x => x.RegistrationNumber).NotEmpty().MaximumLength(32);
        RuleFor(x => x.Make).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Model).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Year).InclusiveBetween(1980, DateTime.UtcNow.Year + 1);
        RuleFor(x => x.DailyRate).GreaterThan(0);
    }
}

public sealed class CreateVehicleCommandHandler : IRequestHandler<CreateVehicleCommand, VehicleDto>
{
    private readonly IVehicleRepository _vehicles;
    private readonly IUnitOfWork _unitOfWork;

    public CreateVehicleCommandHandler(IVehicleRepository vehicles, IUnitOfWork unitOfWork)
    {
        _vehicles = vehicles;
        _unitOfWork = unitOfWork;
    }

    public async Task<VehicleDto> Handle(CreateVehicleCommand request, CancellationToken cancellationToken)
    {
        if (await _vehicles.RegistrationExistsAsync(request.RegistrationNumber, null, cancellationToken))
        {
            throw new DomainException("Registration number must be unique.");
        }

        var vehicle = Vehicle.Create(request.RegistrationNumber, request.Make, request.Model, request.Year, request.DailyRate);
        await _vehicles.AddAsync(vehicle, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return vehicle.ToDto();
    }
}

public sealed record UpdateVehicleCommand(
    Guid Id,
    string RegistrationNumber,
    string Make,
    string Model,
    int Year,
    decimal DailyRate) : IRequest<VehicleDto>;

public sealed class UpdateVehicleCommandValidator : AbstractValidator<UpdateVehicleCommand>
{
    public UpdateVehicleCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.RegistrationNumber).NotEmpty().MaximumLength(32);
        RuleFor(x => x.Make).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Model).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Year).InclusiveBetween(1980, DateTime.UtcNow.Year + 1);
        RuleFor(x => x.DailyRate).GreaterThan(0);
    }
}

public sealed class UpdateVehicleCommandHandler : IRequestHandler<UpdateVehicleCommand, VehicleDto>
{
    private readonly IVehicleRepository _vehicles;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateVehicleCommandHandler(IVehicleRepository vehicles, IUnitOfWork unitOfWork)
    {
        _vehicles = vehicles;
        _unitOfWork = unitOfWork;
    }

    public async Task<VehicleDto> Handle(UpdateVehicleCommand request, CancellationToken cancellationToken)
    {
        var vehicle = await _vehicles.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new DomainException("Vehicle not found.");

        if (await _vehicles.RegistrationExistsAsync(request.RegistrationNumber, request.Id, cancellationToken))
        {
            throw new DomainException("Registration number must be unique.");
        }

        vehicle.Update(request.RegistrationNumber, request.Make, request.Model, request.Year, request.DailyRate);
        _vehicles.Update(vehicle);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return vehicle.ToDto();
    }
}

public sealed record SoftDeleteVehicleCommand(Guid Id) : IRequest;

public sealed class SoftDeleteVehicleCommandHandler : IRequestHandler<SoftDeleteVehicleCommand>
{
    private readonly IVehicleRepository _vehicles;
    private readonly IUnitOfWork _unitOfWork;

    public SoftDeleteVehicleCommandHandler(IVehicleRepository vehicles, IUnitOfWork unitOfWork)
    {
        _vehicles = vehicles;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(SoftDeleteVehicleCommand request, CancellationToken cancellationToken)
    {
        var vehicle = await _vehicles.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new DomainException("Vehicle not found.");

        vehicle.SoftDelete();
        _vehicles.Update(vehicle);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
