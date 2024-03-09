using AspNetWebService.Models;
using AspNetWebService.Models.DataTransferObjectModels;
using AutoMapper;

namespace AspNetWebService.Mapping
{
    /// <summary>
    ///     AutoMapper profile to define mappings between entity classes and DTOs.
    ///     @Author: Christian Briglio
    /// </summary>
    public class AutoMapperProfile : Profile
    {
        /// <summary>
        ///     Constructor to define AutoMapper mappings.
        /// </summary>
        public AutoMapperProfile()
        {
            // Map Users to User Data Transfer Objects
            CreateMap<User, UserDTO>();
        }
    }
}
