using AutoMapper;

using StuffTracker.Domain.Entities;

namespace StuffTracker.Application.Locations.Dtos;

public class LocationsProfile : Profile
{
    public LocationsProfile()
    {
        CreateMap<Location, LocationDto>();
    }
}
