using AutoMapper;
using VehicleServiceManagement.API.Core.Entities;
using VehicleServiceManagement.API.Application.DTOs;
using VehicleServiceManagement.API.Application.DTOs.Auth;

namespace VehicleServiceManagement.API.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Vehicle, VehicleDto>()
                .ForMember(dest => dest.OwnerFullName, opt => opt.MapFrom(src => src.Owner != null ? src.Owner.FullName : "Unknown"));

            CreateMap<ServiceRequest, ServiceRequestDto>()
                .ForMember(dest => dest.VehicleModel, opt => opt.MapFrom(src => src.Vehicle != null ? src.Vehicle.Model : "N/A"))
                .ForMember(dest => dest.TechnicianName, opt => opt.MapFrom(src => src.Technician != null ? src.Technician.FullName : "Unassigned"));

            CreateMap<Part, PartDto>().ReverseMap();


            CreateMap<RegisterRequestDto, ApplicationUser>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email));
        }
    }
}