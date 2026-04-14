using AutoMapper;

using MediatR;

using StuffTracker.Application.Locations.Dtos;
using StuffTracker.Domain.Entities;
using StuffTracker.Domain.Exceptions;
using StuffTracker.Domain.Repositories;

namespace StuffTracker.Application.Locations.Queries.GetLocationById
{
    public class GetLocationsByIdQueryHandler(ILocationsRepository locationRepository, IMapper mapper) : IRequestHandler<GetLocationByIdQuery, LocationDto>
    {
        public async Task<LocationDto> Handle(GetLocationByIdQuery request, CancellationToken cancellationToken)
        {
            Location? location = await locationRepository.GetLocationById(request.LocationId) ??
                throw new NotFoundException(nameof(Location), request.LocationId.ToString());

            if (location.HomeId != request.HomeId)
                throw new NotFoundException(nameof(Location), request.LocationId.ToString());

            return mapper.Map<LocationDto>(location);
        }
    }
}