using Bruno.Application.Common.Dtos;
using Bruno.Application.Common.Interfaces;
using Bruno.Application.Common.Mappings;
using Bruno.Domain.Common;
using Bruno.Domain.Entities;
using FluentValidation;
using MediatR;

namespace Bruno.Application.Customers.Commands;

public sealed record CreateCustomerCommand(
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber) : IRequest<CustomerDto>;

public sealed class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(40);
    }
}

public sealed class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, CustomerDto>
{
    private readonly ICustomerRepository _customers;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCustomerCommandHandler(ICustomerRepository customers, IUnitOfWork unitOfWork)
    {
        _customers = customers;
        _unitOfWork = unitOfWork;
    }

    public async Task<CustomerDto> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        if (await _customers.EmailExistsAsync(request.Email, null, cancellationToken))
        {
            throw new DomainException("Email must be unique.");
        }

        var customer = Customer.Create(request.FirstName, request.LastName, request.Email, request.PhoneNumber);
        await _customers.AddAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return customer.ToDto();
    }
}

public sealed record UpdateCustomerCommand(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber) : IRequest<CustomerDto>;

public sealed class UpdateCustomerCommandValidator : AbstractValidator<UpdateCustomerCommand>
{
    public UpdateCustomerCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(40);
    }
}

public sealed class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, CustomerDto>
{
    private readonly ICustomerRepository _customers;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCustomerCommandHandler(ICustomerRepository customers, IUnitOfWork unitOfWork)
    {
        _customers = customers;
        _unitOfWork = unitOfWork;
    }

    public async Task<CustomerDto> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _customers.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new DomainException("Customer not found.");

        if (await _customers.EmailExistsAsync(request.Email, request.Id, cancellationToken))
        {
            throw new DomainException("Email must be unique.");
        }

        customer.Update(request.FirstName, request.LastName, request.Email, request.PhoneNumber);
        _customers.Update(customer);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return customer.ToDto();
    }
}

public sealed record DeleteCustomerCommand(Guid Id) : IRequest;

public sealed class DeleteCustomerCommandHandler : IRequestHandler<DeleteCustomerCommand>
{
    private readonly ICustomerRepository _customers;
    private readonly IBookingRepository _bookings;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCustomerCommandHandler(
        ICustomerRepository customers,
        IBookingRepository bookings,
        IUnitOfWork unitOfWork)
    {
        _customers = customers;
        _bookings = bookings;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _customers.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new DomainException("Customer not found.");

        var hasBookings = await _bookings.CustomerHasAnyBookingsAsync(request.Id, cancellationToken);
        customer.EnsureCanBeDeleted(hasBookings);

        _customers.Remove(customer);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
