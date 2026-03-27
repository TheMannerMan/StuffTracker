using MediatR;

namespace StuffTracker.Application.Locations.Queries.GetLocationsForHome;

public record GetLocationsForHomeQuery(int homeId) : IRequest<IEnumerable<LocationDto>>
{
    public Guid HomeId { get; init; }

}
