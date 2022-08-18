using System.ComponentModel.DataAnnotations;

namespace Palantir.Identity.Models
{
    public sealed class LoginRequest
    {
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        public bool RememberMe { get; set; }

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
