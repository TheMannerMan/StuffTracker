using MediatR;

using StuffTracker.Application.Locations.Dtos;

namespace StuffTracker.Application.Locations.Queries.GetHomes;

public record class GetHomesQuery : IRequest<IEnumerable<HomeDto>>;
