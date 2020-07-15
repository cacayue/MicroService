using System.Collections.Generic;
using System.Security.Claims;
using IdentityModel;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Test;

namespace User.Identity
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
            };
        }

        public static IEnumerable<ApiScope> ApiScopes =>
            new ApiScope[]
            {
                new ApiScope("gateway_scope"),
                new ApiScope("contact_scope"),
                new ApiScope("user_scope"),
                new ApiScope("project_scope"),
                new ApiScope("recommend_scope"),
            };

        public static IEnumerable<ApiResource> GetApis()
        {
            return new ApiResource[]
            {
                new ApiResource("gateway_api", "gateway")
                {
                    //!!!重要
                    Scopes = { "gateway_scope" }
                },
                new ApiResource("contact_api", "contact service")
                {
                    //!!!重要
                    Scopes = { "contact_scope" }
                },
                new ApiResource("user_api", "user service")
                {
                    //!!!重要
                    Scopes = { "user_scope" }
                },
                new ApiResource("project_api", "project service")
                {
                    //!!!重要
                    Scopes = { "project_scope" }
                },
                new ApiResource("recommend_api", "recommend service")
                {
                    //!!!重要
                    Scopes = { "recommend_scope" }
                },
            };
        }

        public static IEnumerable<Client> GetClients()
        {
            return new[]
            {
                // Custom Auth
                new Client
                {
                    ClientId = "android",
                    ClientSecrets = {new Secret("secret".Sha256()) },
                    RefreshTokenExpiration = TokenExpiration.Sliding,
                    AllowOfflineAccess = true,
                    RequireClientSecret = false,
                    AllowedGrantTypes = new List<string>(){"sms_auth_code"},
                    AlwaysIncludeUserClaimsInIdToken = true,
                    AllowedScopes = {
                        "gateway_scope",
                        "contact_scope",
                        "user_scope",
                        "project_scope",
                        "recommend_scope",
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.OfflineAccess,
                    }
                }
            };
        }
    }
}