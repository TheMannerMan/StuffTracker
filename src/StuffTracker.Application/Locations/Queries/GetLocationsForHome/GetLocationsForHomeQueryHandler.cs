using AutoMapper;
using MediatR;
using StuffTracker.Application.Locations.Dtos;
using StuffTracker.Domain.Repositories;

namespace StuffTracker.Application.Locations.Queries.GetLocationsForHome;

public class GetLocationsForHomeQueryHandler(ILocationsRepository locationRepository, IMapper mapper) : IRequestHandler<GetLocationsForHomeQuery, IEnumerable<LocationDto>>
{
    public Task<IEnumerable<LocationDto>> Handle(GetLocationsForHomeQuery request, CancellationToken cancellationToken)
    {
        var locations = locationRepository.GetLocationsForHome(request.HomeId);
        var locationDtos = mapper.Map<IEnumerable<LocationDto>>(locations); // har jag skapat mapping ännu?
        return Task.FromResult(locationDtos);

    }
}