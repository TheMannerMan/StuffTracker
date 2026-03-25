using MediatR;

using StuffTracker.Domain.Entities;
using StuffTracker.Domain.Exceptions;
using StuffTracker.Domain.Repositories;

namespace StuffTracker.Application.Locations.Commands.CreateLocation;

public class CreateLocationCommandHandler(
    ILocationsRepository locationsRepository) : IRequestHandler<CreateLocationCommand, Guid>
{
    public async Task<Guid> Handle(CreateLocationCommand request, CancellationToken cancellationToken)
    {

        var parentLocation = await locationsRepository.GetLocationById(request.ParentId)??
            throw new NotFoundException(nameof(Location), request.ParentId.ToString());

        var newLocation = Location.CreateLocation(request.Name, request.Description, request.LocationType, parentLocation);

        await locationsRepository.Create(newLocation);
        return newLocation.Id;
    }
}