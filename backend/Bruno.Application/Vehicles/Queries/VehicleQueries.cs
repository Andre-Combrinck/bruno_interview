using Bruno.Application.Common.Dtos;
using Bruno.Application.Common.Interfaces;
using Bruno.Application.Common.Mappings;
using Bruno.Application.Common.Models;
using Bruno.Domain.Common;
using MediatR;

namespace Bruno.Application.Vehicles.Queries;

public sealed record GetVehicleByIdQuery(Guid Id, bool IncludeDeleted = false) : IRequest<VehicleDto>;

public sealed class GetVehicleByIdQueryHandler : IRequestHandler<GetVehicleByIdQuery, VehicleDto>
{
    private readonly IVehicleRepository _vehicles;

    public GetVehicleByIdQueryHandler(IVehicleRepository vehicles)
    {
        _vehicles = vehicles;
    }

    public async Task<VehicleDto> Handle(GetVehicleByIdQuery request, CancellationToken cancellationToken)
    {
        var vehicle = request.IncludeDeleted
            ? await _vehicles.GetByIdIncludingDeletedAsync(request.Id, cancellationToken)
            : await _vehicles.GetByIdAsync(request.Id, cancellationToken);

        return vehicle?.ToDto() ?? throw new DomainException("Vehicle not found.");
    }
}

public sealed record GetVehiclesQuery(
    string? Search,
    bool IncludeDeleted,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<VehicleDto>>;

public sealed class GetVehiclesQueryHandler : IRequestHandler<GetVehiclesQuery, PagedResult<VehicleDto>>
{
    private readonly IVehicleRepository _vehicles;

    public GetVehiclesQueryHandler(IVehicleRepository vehicles)
    {
        _vehicles = vehicles;
    }

    public async Task<PagedResult<VehicleDto>> Handle(GetVehiclesQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize is < 1 or > 100 ? 20 : request.PageSize;

        var (items, total) = await _vehicles.GetPagedAsync(request.Search, request.IncludeDeleted, page, pageSize, cancellationToken);

        return new PagedResult<VehicleDto>
        {
            Items = items.Select(v => v.ToDto()).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }
}
