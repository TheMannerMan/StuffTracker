using MediatR;

using StuffTracker.Application.Locations.Dtos;

namespace StuffTracker.Application.Locations.Queries.GetLocationsForHome;

public record GetLocationsForHomeQuery(Guid HomeId) : IRequest<IEnumerable<LocationDto>>;
