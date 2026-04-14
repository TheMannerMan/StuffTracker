using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MediatR;

using StuffTracker.Application.Locations.Dtos;

namespace StuffTracker.Application.Locations.Queries.GetLocationById;

public record GetLocationByIdQuery(Guid LocationId, Guid HomeId) : IRequest<LocationDto>;

