using AutoMapper;

using MediatR;

using StuffTracker.Application.Locations.Dtos;
using StuffTracker.Domain.Repositories;

namespace StuffTracker.Application.Locations.Queries.GetHomes;

internal class GetHomesQueryHandler(ILocationsRepository locationsRepository, IMapper mapper)
    : IRequestHandler<GetHomesQuery, IEnumerable<HomeDto>>
{
    public async Task<IEnumerable<HomeDto>> Handle(GetHomesQuery request, CancellationToken cancellationToken)
    {
        var homes = await locationsRepository.GetAllHomes();
        return mapper.Map<IEnumerable<HomeDto>>(homes);
    }
}
