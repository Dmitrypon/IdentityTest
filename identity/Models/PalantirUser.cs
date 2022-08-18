using Microsoft.AspNetCore.Identity;

namespace Palantir.Identity.Models
{
    public class PalantirUser : IdentityUser
    {
        public bool IsApproved { get; set; }
    }
}
