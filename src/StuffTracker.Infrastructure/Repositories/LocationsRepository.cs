using Microsoft.EntityFrameworkCore;

using StuffTracker.Domain.Entities;
using StuffTracker.Domain.Repositories;
using StuffTracker.Infrastructure.Persistance;

namespace StuffTracker.Infrastructure.Repositories;

internal class LocationsRepository(StuffTrackerDbContext dbContext) : ILocationsRepository
{
    public async Task<Guid> Create(Location entity)
    {
        dbContext.Locations.Add(entity);
        await dbContext.SaveChangesAsync();
        return entity.Id;
    }

    public async Task<Location?> GetLocationById(Guid id)
    {
        var location = await dbContext.Locations.FirstOrDefaultAsync(l => l.Id == id && !l.IsDeleted);
        return location;
    }
}
