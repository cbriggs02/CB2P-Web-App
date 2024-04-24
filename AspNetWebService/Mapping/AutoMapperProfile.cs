using AspNetWebService.Models;
using AspNetWebService.Models.DataTransferObjectModels;
using AutoMapper;

namespace AspNetWebService.Mapping
{
    /// <summary>
    ///     AutoMapper profile to define mappings between entity classes and DTOs.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    /// </remarks>
    public class AutoMapperProfile : Profile
    {
        /// <summary>
        ///     Constructor to define AutoMapper mappings.
        /// </summary>
        public AutoMapperProfile()
        {
            CreateMap<User, UserDTO>();
        }
    }
}
