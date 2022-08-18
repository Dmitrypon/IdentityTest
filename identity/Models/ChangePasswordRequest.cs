using System.ComponentModel.DataAnnotations;

namespace Palantir.Identity.Models
{
    public sealed class ChangePasswordRequest
    {
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        
        [Required]
        public string Token { get; set; }
        
        [Required, DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
