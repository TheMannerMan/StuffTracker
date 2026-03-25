using System;
using System.Collections.Generic;
using System.Text;

using AutoMapper;

using MediatR;

using StuffTracker.Application.Locations.Dtos;
using StuffTracker.Domain.Entities;
using StuffTracker.Domain.Enums;
using StuffTracker.Domain.Exceptions;
using StuffTracker.Domain.Repositories;

namespace StuffTracker.Application.Locations.Queries.GetHomeById;

internal class GetHomeByIdQueryHandler(ILocationsRepository locationsRepository, IMapper mapper) : IRequestHandler<GetHomeByIdQuery, HomeDto>
{
    public async Task<HomeDto> Handle(GetHomeByIdQuery request, CancellationToken cancellationToken)
    {
        Location home = await locationsRepository.GetLocationById(request.Id) ??
            throw new NotFoundException(nameof(Location), request.Id.ToString());

        if (home.LocationType != LocationType.Home)
            throw new NotFoundException(nameof(Location), request.Id.ToString());

        var homeDto = mapper.Map<HomeDto>(home);
        return homeDto;
    }
}
