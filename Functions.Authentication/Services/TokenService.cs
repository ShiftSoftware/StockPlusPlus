using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Functions.Authentication.Services;

internal class TokenService
{
    private readonly IEnumerable<AuthenticationOptions> listAuthenticationOptions;

    public TokenService(IEnumerable<AuthenticationOptions> listAuthenticationOptions)
    {
        this.listAuthenticationOptions = listAuthenticationOptions;
    }

    internal Dictionary<string, ClaimsPrincipal?> ValidateToken(string? token)
    {
        Dictionary<string, ClaimsPrincipal?> claims = new();

        foreach (var authenticationOptions in this.listAuthenticationOptions)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var principal = tokenHandler.ValidateToken(token, authenticationOptions.TokenValidationParameters
                    , out SecurityToken securityToken);
                JwtSecurityToken jwtSecurityToken = securityToken as JwtSecurityToken;

                if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha512Signature, StringComparison.InvariantCultureIgnoreCase))
                    claims.Add(authenticationOptions.AuthenticationScheme ?? "", null);
                else
                    claims.Add(authenticationOptions.AuthenticationScheme ?? "", principal);
            }
            catch (Exception)
            {
                claims.Add(authenticationOptions.AuthenticationScheme ?? "", null);
            }
        }

        return claims;
    }
}
