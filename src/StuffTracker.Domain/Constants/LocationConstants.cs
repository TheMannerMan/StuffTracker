using StuffTracker.Domain.Enums;

namespace StuffTracker.Domain.Constants;

public static class LocationConstants
{
    public const int LocationNameMaxLength = 100;
    public const int LocationDescriptionMaxLength = 500;

    public static readonly IReadOnlyList<LocationType> ValidLocationTypesToBeCreated = new List<LocationType>()
    {
        LocationType.Storage,
        LocationType.Room
    };
}