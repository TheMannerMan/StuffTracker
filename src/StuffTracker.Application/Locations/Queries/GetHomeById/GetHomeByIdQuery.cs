using MediatR;

using StuffTracker.Application.Locations.Dtos;

namespace StuffTracker.Application.Locations.Queries.GetHome;

public record class GetHomeByIdQuery(Guid Id) : IRequest<HomeDto>;
