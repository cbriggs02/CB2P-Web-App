using System.ComponentModel.DataAnnotations;

namespace AspNetWebService.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class Login
    {
        /// <summary>
        /// 
        /// </summary>
        [Required]
        public string UserName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Required] 
        public string Password { get;}
    }
}
