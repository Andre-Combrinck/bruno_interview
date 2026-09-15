using Bruno.Application.Common.Dtos;
using Bruno.Application.Common.Interfaces;
using Bruno.Application.Common.Mappings;
using Bruno.Application.Common.Models;
using Bruno.Domain.Common;
using MediatR;

namespace Bruno.Application.Customers.Queries;

public sealed record GetCustomerByIdQuery(Guid Id) : IRequest<CustomerDto>;

public sealed class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, CustomerDto>
{
    private readonly ICustomerRepository _customers;

    public GetCustomerByIdQueryHandler(ICustomerRepository customers)
    {
        _customers = customers;
    }

    public async Task<CustomerDto> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var customer = await _customers.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new DomainException("Customer not found.");

        return customer.ToDto();
    }
}

public sealed record GetCustomersQuery(
    string? Search,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<CustomerDto>>;

public sealed class GetCustomersQueryHandler : IRequestHandler<GetCustomersQuery, PagedResult<CustomerDto>>
{
    private readonly ICustomerRepository _customers;

    public GetCustomersQueryHandler(ICustomerRepository customers)
    {
        _customers = customers;
    }

    public async Task<PagedResult<CustomerDto>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize is < 1 or > 100 ? 20 : request.PageSize;

        var (items, total) = await _customers.GetPagedAsync(request.Search, page, pageSize, cancellationToken);

        return new PagedResult<CustomerDto>
        {
            Items = items.Select(c => c.ToDto()).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }
}
