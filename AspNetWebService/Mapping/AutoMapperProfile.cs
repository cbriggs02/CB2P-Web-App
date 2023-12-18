using AutoMapper;
using AspNetWebService.Models;

namespace AspNetWebService.Mapping
{
    /// <summary>
    /// AutoMapper profile to define mappings between entity classes and DTOs.
    /// </summary>
    public class AutoMapperProfile : Profile
    {
        /// <summary>
        /// Constructor to define AutoMapper mappings.
        /// </summary>
        public AutoMapperProfile()
        {
            // Map Users to User Data Transfer Objects
            CreateMap<User, UserDTO>();
        }
    }
}
