using MediatR;

using StuffTracker.Domain.Entities;
using StuffTracker.Domain.Repositories;

namespace StuffTracker.Application.Locations.Commands.CreateHome;

public class CreateHomeCommandHandler(
    ILocationsRepository locationsRepository) : IRequestHandler<CreateHomeCommand, Guid>
{
    public async Task<Guid> Handle(CreateHomeCommand request, CancellationToken cancellationToken)
    {
        var (home, unsorted) = Location.CreateHome(request.Name, request.Description);
        await locationsRepository.CreateRange([home, unsorted]);
        return home.Id;
    }
}