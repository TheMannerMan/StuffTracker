using Microsoft.EntityFrameworkCore;

using StuffTracker.Domain.Entities;
using StuffTracker.Domain.Enums;
using StuffTracker.Domain.Repositories;
using StuffTracker.Infrastructure.Persistence;

namespace StuffTracker.Infrastructure.Repositories;

internal class LocationsRepository(StuffTrackerDbContext dbContext) : ILocationsRepository
{
    public async Task<Guid> Create(Location entity)
    {
        dbContext.Locations.Add(entity);
        await dbContext.SaveChangesAsync();
        return entity.Id;
    }

    public async Task CreateRange(IEnumerable<Location> entities)
    {
        dbContext.Locations.AddRange(entities);
        await dbContext.SaveChangesAsync();
    }

    public async Task<Location?> GetLocationById(Guid id)
    {
        var location = await dbContext.Locations.FirstOrDefaultAsync(l => l.Id == id);
        return location;
    }

    //TODO: vad är syftet med deb här? Rimligtvis så ska man hämta alla hem för en specifik användare? Kontrollera
    public async Task<IEnumerable<Location>> GetAllHomes()
    {
        return await dbContext.Locations
            .Where(l => l.LocationType == LocationType.Home)
            .ToListAsync();
    }

    public async Task<IEnumerable<Location>> GetLocationsForHome(Guid homeId)
    {
        var locations = await dbContext.Locations
            .AsNoTracking()
            .Where(l => l.HomeId == homeId)
            .ToListAsync();

        return locations;
    }
}
