using System;
using Microsoft.AspNetCore.Identity;

namespace Palantir.Identity.Models
{
    public enum PalantirClaimValue
    {
        View,
        Edit
    }

    public class PalantirUserClaim : IdentityUserClaim<long>
    {

    }
}
