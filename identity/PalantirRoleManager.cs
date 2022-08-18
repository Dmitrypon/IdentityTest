using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Palantir.Identity
{
    public class PalantirRoleManager : RoleManager<IdentityRole>
    {
        public PalantirRoleManager(IRoleStore<IdentityRole> store, IEnumerable<IRoleValidator<IdentityRole>> roleValidators, ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors, ILogger<RoleManager<IdentityRole>> logger) : base(store, roleValidators, keyNormalizer, errors, logger)
        {
        }
    }
}
