using Functions.Authentication.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Functions.Authentication;

public class AuthenticationBuilder
{
    private readonly IFunctionsWorkerApplicationBuilder builder;

    public AuthenticationBuilder(IFunctionsWorkerApplicationBuilder builder)
    {
        this.builder = builder;
    }

    public AuthenticationBuilder AddJwtBearer(string authenticationScheme,
        TokenValidationParameters tokenValidationParameters)
    {
        builder.UseMiddleware<AuthenticationMiddleware>();
        builder.Services.AddScoped(x => new AuthenticationOptions(tokenValidationParameters, authenticationScheme));
        builder.Services.AddScoped<TokenService>();

        return this;
    }

    public AuthenticationBuilder AddJwtBearer(TokenValidationParameters tokenValidationParameters)
    {
        return this.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, tokenValidationParameters);
    }

    public AuthenticationBuilder AddJwtBearer(Action<TokenValidationParameters> tokenValidationParametersBuilder)
    {
        var tokenValidationParameters = new TokenValidationParameters();
        tokenValidationParametersBuilder(tokenValidationParameters);

        return this.AddJwtBearer(tokenValidationParameters);
    }

    public AuthenticationBuilder AddJwtBearer(string authenticationScheme,
        Action<TokenValidationParameters> tokenValidationParametersBuilder)
    {
        var tokenValidationParameters = new TokenValidationParameters();
        tokenValidationParametersBuilder(tokenValidationParameters);

        return this.AddJwtBearer(authenticationScheme, tokenValidationParameters);
    }
}
