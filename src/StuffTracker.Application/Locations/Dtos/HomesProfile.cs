using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using AutoMapper;

using StuffTracker.Application.Locations.Commands.CreateHome;
using StuffTracker.Domain.Entities;
using StuffTracker.Domain.Enums;

namespace StuffTracker.Application.Locations.Dtos;

public class HomesProfile : Profile
{
    public HomesProfile()
    {
        CreateMap<CreateHomeCommand, Location>()
              .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
              .ForMember(dest => dest.LocationType, opt => opt.MapFrom(_ => LocationType.Home))
              .ForMember(dest => dest.ParentId, opt => opt.Ignore())
              .ForMember(dest => dest.HomeId, opt => opt.Ignore())
              .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
              .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
              .ForMember(dest => dest.ImageUrl, opt => opt.Ignore())
              .ForMember(dest => dest.Parent, opt => opt.Ignore())
              .ForMember(dest => dest.Children, opt => opt.Ignore())
              .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
              // AfterMap runs after all ForMember mappings are complete. We set HomeId here
              // because it must equal the generated Id, which is only available after mapping.
              .AfterMap((_, dest) => dest.HomeId = dest.Id);

        CreateMap<Location, HomeDto>();
    }
}      