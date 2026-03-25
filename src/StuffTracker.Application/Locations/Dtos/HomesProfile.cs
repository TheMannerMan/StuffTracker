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
        CreateMap<Location, HomeDto>();
    }
}      