using AutoMapper;
using family_registry_backend.DTOs;
using family_registry_backend.Models;

namespace family_registry_backend.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Employee mappings
        CreateMap<Employee, EmployeeResponseDto>();
        CreateMap<EmployeeCreateDto, Employee>()
            .ForMember(dest => dest.Spouse, opt => opt.Ignore())
            .ForMember(dest => dest.Children, opt => opt.Ignore());
        CreateMap<EmployeeUpdateDto, Employee>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Spouse, opt => opt.Ignore())
            .ForMember(dest => dest.Children, opt => opt.Ignore());

        // Spouse mappings
        CreateMap<Spouse, SpouseResponseDto>();
        CreateMap<SpouseDto, Spouse>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.EmployeeId, opt => opt.Ignore())
            .ForMember(dest => dest.Employee, opt => opt.Ignore());

        // Child mappings
        CreateMap<Child, ChildResponseDto>();
        CreateMap<ChildDto, Child>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.EmployeeId, opt => opt.Ignore())
            .ForMember(dest => dest.Employee, opt => opt.Ignore());
    }
}
