//Основной профиль AutoMapper
using AutoMapper;
using ElectronicsComponentWarehouse.Application.DTOs.Auth;
using ElectronicsComponentWarehouse.Application.DTOs.Categories;
using ElectronicsComponentWarehouse.Application.DTOs.Components;
using ElectronicsComponentWarehouse.Application.DTOs.Users;
using ElectronicsComponentWarehouse.Domain.Entities;

namespace ElectronicsComponentWarehouse.Application.Mappings
{
    /// <summary>
    /// Профиль AutoMapper для преобразования между сущностями и DTO
    /// </summary>
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Component mappings
            CreateMap<Component, ComponentDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
                .ReverseMap();
            
            CreateMap<CreateComponentDto, Component>();
            CreateMap<UpdateComponentDto, Component>();
            
            // Category mappings
            CreateMap<Category, CategoryDto>()
                .ForMember(dest => dest.ParentCategoryName, opt => opt.MapFrom(src => src.ParentCategory != null ? src.ParentCategory.Name : null))
                .ForMember(dest => dest.ComponentCount, opt => opt.Ignore())
                .ForMember(dest => dest.ChildCategoryCount, opt => opt.Ignore())
                .ForMember(dest => dest.ChildCategories, opt => opt.MapFrom(src => src.ChildCategories))
                .ReverseMap()
                .ForMember(dest => dest.ParentCategory, opt => opt.Ignore())
                .ForMember(dest => dest.ChildCategories, opt => opt.Ignore());
            
            CreateMap<CreateCategoryDto, Category>();
            CreateMap<UpdateCategoryDto, Category>();
            
            // User mappings
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()))
                .ReverseMap()
                .ForMember(dest => dest.Role, opt => opt.Ignore()); // Роль устанавливается отдельно
            
            CreateMap<CreateUserDto, User>()
                .ForMember(dest => dest.Role, opt => opt.Ignore()) // Роль устанавливается отдельно
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordSalt, opt => opt.Ignore());
            
            CreateMap<User, UserInfoDto>()
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()));
            
            // Auth mappings
            CreateMap<User, AuthResponseDto>()
                .ForMember(dest => dest.User, opt => opt.MapFrom(src => src));
        }
    }
}
