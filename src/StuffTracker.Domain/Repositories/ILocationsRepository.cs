using System;
using System.Collections.Generic;
using System.Text;

using StuffTracker.Domain.Entities;

namespace StuffTracker.Domain.Repositories;

public interface ILocationsRepository
{
    Task<Guid> Create(Location entity);
    Task CreateRange(IEnumerable<Location> entities);
    Task<Location?> GetLocationById(Guid id);
    Task<IEnumerable<Location>> GetAllHomes();
}
