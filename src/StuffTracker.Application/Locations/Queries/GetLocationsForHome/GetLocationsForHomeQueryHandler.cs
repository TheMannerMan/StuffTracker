using AutoMapper;
using MediatR;
using StuffTracker.Application.Locations.Dtos;
using StuffTracker.Domain.Repositories;

namespace StuffTracker.Application.Locations.Queries.GetLocationsForHome;

public class GetLocationsForHomeQueryHandler(ILocationsRepository locationRepository, IMapper mapper)
    : IRequestHandler<GetLocationsForHomeQuery, IEnumerable<LocationDto>>
{
    public async Task<IEnumerable<LocationDto>> Handle(GetLocationsForHomeQuery request, CancellationToken cancellationToken)
    {
        var locations = await locationRepository.GetLocationsForHome(request.HomeId);
        return mapper.Map<IEnumerable<LocationDto>>(locations);
    }
}
