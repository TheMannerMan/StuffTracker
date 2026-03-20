using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StuffTracker.Domain.Repositories;

using AutoMapper;

using MediatR;

using StuffTracker.Domain.Entities;

namespace StuffTracker.Application.Locations.Commands.CreateHome
{
    public class CreateHomeCommandHandler(
        IMapper mapper, ILocationsRepository locationsRepository) : IRequestHandler<CreateHomeCommand, Guid>
    {
        public async Task<Guid> Handle(CreateHomeCommand request, CancellationToken cancellationToken)
        {
            var home = mapper.Map<Location>(request);
            Guid id = await locationsRepository.Create(home);
            return id;
            // TODO: where should the Guid be generated? In the handler or in the repository? If in the repository, should it be returned by the Create method or should it be generated in the handler and passed to the Create method?
            // Jag vill inte returnnera id om transaktion inte gått igenom hos databasen.
        }
    }
}