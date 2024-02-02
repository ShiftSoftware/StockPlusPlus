using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Functions.Authentication;

internal class AuthenticationOptions
{
    public TokenValidationParameters TokenValidationParameters { get; internal set; }
    public string? AuthenticationScheme { get; internal set; }

    public AuthenticationOptions(TokenValidationParameters tokenValidationParameters, string scheme)
    {
        this.TokenValidationParameters = tokenValidationParameters;
        this.AuthenticationScheme = scheme;
    }
}
