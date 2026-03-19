using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MediatR;

namespace StuffTracker.Application.Locations.Commands.CreateHome;

public record CreateHomeCommand : IRequest<Guid> 
  {
      public required string Name { get; init; }
      public string? Description { get; init; }    
      // TODO: Prepared for Step 2 (image upload     via Azure Blob Storage)                          
      //public string? ImageUrl { get; init; }     
  }  