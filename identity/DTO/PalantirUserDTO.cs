using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Palantir.Identity.Models;

namespace Palantir.Identity.DTO
{
    public class PalantirUserDTO
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public bool? IsApproved { get; set; }
        public IList<ClaimDTO> Claims { get; set; }

        public PalantirUserDTO() { }
        public PalantirUserDTO(PalantirUser user, string role, IList<Claim> claims = null)
        {
            Id = user.Id;
            UserName = user.UserName;
            Email = user.Email;
            Role = role;
            IsApproved = user.IsApproved;
            Claims = claims?.Select(c => new ClaimDTO(c)).ToList();
        }
    }

    public sealed class ClaimDTO
    {
        public string ProjectId { get; set; }
        public PalantirClaimValue Claim { get; set; }

        public ClaimDTO() { }

        public ClaimDTO(Claim claim)
        {
            ProjectId = claim.Type;
            Claim = Enum.Parse<PalantirClaimValue>(claim.Value);
        }
    }
}
