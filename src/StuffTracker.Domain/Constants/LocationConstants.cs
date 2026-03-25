using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using StuffTracker.Domain.Enums;

namespace StuffTracker.Domain.Constants
{
    public static class LocationConstants
    {
        public const int LocationNameMaxLength = 100;
        public const int LocationDescriptionMaxLength = 500;

        public static readonly List<LocationType> ValidLocationTypesToBeCreated = new List<LocationType>()
        {
            LocationType.Storage,
            LocationType.Room
        };
    }
}