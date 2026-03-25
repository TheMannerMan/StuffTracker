using MediatR;

using StuffTracker.Domain.Enums;

namespace StuffTracker.Application.Locations.Commands.CreateLocation
{
    public record CreateLocationCommand : IRequest<Guid>
    {
        public required string Name { get; init; }
        public string? Description { get; init; }
        public required Guid ParentId { get; init; }
        public required LocationType LocationType { get; init; }

        // TODO: Prepared for Step 2 (image upload     via Azure Blob Storage)                          
      //public string? ImageUrl { get; init; }  

    }
}