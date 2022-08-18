using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Palantir.Identity.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Palantir.Identity
{
    public class PalantirUserManager : UserManager<PalantirUser>
    {
        public PalantirUserManager(IUserStore<PalantirUser> store, IOptions<IdentityOptions> optionsAccessor, IPasswordHasher<PalantirUser> passwordHasher, IEnumerable<IUserValidator<PalantirUser>> userValidators, IEnumerable<IPasswordValidator<PalantirUser>> passwordValidators, ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<PalantirUser>> logger) : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
        }
    }
}
