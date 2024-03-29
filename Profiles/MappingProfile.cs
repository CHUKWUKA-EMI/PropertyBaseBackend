﻿using System;
using AutoMapper;
using PropertyBase.DTOs.Property;
using PropertyBase.DTOs.User;
using PropertyBase.Entities;
using PropertyBase.Features.Properties.UpdateProperty;
using PropertyBase.Features.Properties.GetPropertyDetails;
using PropertyBase.Features.Properties.SaveDraft;

namespace PropertyBase.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<UpdatePropertyRequest, Property>()
                .ForMember(dest=>dest.Id,opt=>opt.MapFrom(src=>src.PropertyId));

            CreateMap<SaveDraftRequest, Property>().ReverseMap();

            CreateMap<User, UserProfileVM>();

            CreateMap<PropertyImage, PropertyImageVM>();

            CreateMap<Agency, PropertyAgencyVM>()
                .ForMember(c => c.Owner, opt => opt.MapFrom(src => src.Owner));

            CreateMap<Property, PropertyOverviewVM>()
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images));
            CreateMap<Property, GetPropertyDetailsResponse>()
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images))
                .ForMember(dest => dest.Owner, opt => opt.MapFrom(src => src.Owner))
                .ForMember(dest => dest.Agency, opt => opt.MapFrom(src => src.Agency));
                
        }
    }
}

