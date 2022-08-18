using IdentityServer4;
using IdentityServer4.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Palantir.Identity
{
    public static class Config
    {
        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
            {
                new ApiResource("Infrastructure", "Palantir Infrastructure")
            };
        }

        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
            };
        }

        public static IEnumerable<Client> GetClients()
        {
            return new[]
            {
                new Client
                {
                    ClientId = "PalantirSPA",
                    ClientName = "SPA Client",
                    AllowedGrantTypes = GrantTypes.Code,

                    RequireConsent = false,

                    ClientSecrets =
                    {
                        new Secret("PalantirSecret".Sha256())
                    },

                    RedirectUris           = { "http://localhost:5002/signin-oidc" },
                    PostLogoutRedirectUris = { "http://localhost:5002/signout-callback-oidc" },

                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "Infrastructure"
                    },
                    AllowOfflineAccess = true,
                    AccessTokenType = AccessTokenType.Reference
                }
            };
        }
    }
}
