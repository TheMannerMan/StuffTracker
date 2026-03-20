using System;
using System.Collections.Generic;
using System.Text;

using AutoMapper;

using MediatR;

using StuffTracker.Application.Locations.Dtos;
using StuffTracker.Domain.Entities;
using StuffTracker.Domain.Enums;
using StuffTracker.Domain.Repositories;

namespace StuffTracker.Application.Locations.Queries.GetHomeById;

internal class GetHomeByIdQueryHandler(ILocationsRepository locationsRepository, IMapper mapper) : IRequestHandler<GetHomeByIdQuery, HomeDto>
{
    public async Task<HomeDto> Handle(GetHomeByIdQuery request, CancellationToken cancellationToken)
    {
        Location home = await locationsRepository.GetLocationById(request.Id) ??
            throw new Exception($"Home with id {request.Id} not found."); // Handle this more gracefully
        
        if (home.LocationType != LocationType.Home)
        {
            throw new Exception($"Location with id {request.Id} is not a home."); // Handle this more gracefully
        }
        var homeDto = mapper.Map<HomeDto>(home);
        return homeDto;
    }
}
