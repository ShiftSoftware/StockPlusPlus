using Functions.Authentication.Extensions;
using Functions.Authentication.Services;
using Functions.Authentication.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Dynamic;
using System.Net;
using System.Security.Claims;

namespace Functions.Authentication;

internal class AuthenticationMiddleware : IFunctionsWorkerMiddleware
{
    private readonly TokenService tokenService;

    public AuthenticationMiddleware(TokenService tokenService)
    {
        this.tokenService = tokenService;
    }

    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        Dictionary<string, ClaimsPrincipal?> shcemeClaims = null;

        //Set user to prevent null reference exception
        context.Items["User"] = new ClaimsPrincipal();

        var request = await context.GetHttpRequestDataAsync();
        var authorizationHeader = request?.Headers?.FirstOrDefault(x => x.Key.ToLower() == "Authorization".ToLower());
        var authorizationHeaderValue = authorizationHeader.GetValueOrDefault().Key is not null ?
            authorizationHeader!.Value.Value.FirstOrDefault() : null;

        // Get the token from the request header
        var token = authorizationHeaderValue?.Replace("Bearer ", "");

        shcemeClaims = tokenService.ValidateToken(token);

        var methodInfo = context.GetTargetFunctionMethod();
        var authoraizeAttribute = AttributeUtility.GetAttribute<AuthorizeAttribute>(methodInfo);
        var anonymousAttribute = AttributeUtility.GetAttribute<AllowAnonymousAttribute>(methodInfo);

        if (HasAuthorizeEffect(authoraizeAttribute, anonymousAttribute))
        {
            var schemes = ParseSchemes(authoraizeAttribute.GetValueOrDefault().attribute?.AuthenticationSchemes);

            ClaimsPrincipal? claims = null;
            if (schemes is not null)
                claims = shcemeClaims.FirstOrDefault(x => schemes.Contains(x.Key) && x.Value is not null).Value;
            else
                claims = shcemeClaims.FirstOrDefault().Value;

            if (claims is null)
            {
                var response = request?.CreateResponse(HttpStatusCode.Unauthorized);
                context.GetInvocationResult().Value = response;
                return;
            }
            else
            {
                var roles = ParseRoles(authoraizeAttribute.GetValueOrDefault().attribute?.Roles);

                if (roles is not null)
                {
                    if(!UserIsInRole(claims, roles))
                    {
                        var response = request?.CreateResponse(HttpStatusCode.Forbidden);
                        context.GetInvocationResult().Value = response;
                        return;
                    }
                }

                context.Items["User"] = claims!;
                request?.Identities.ToList().AddRange(claims?.Identities);

                await next(context);
            }
        }
        else
        {
            var claims = shcemeClaims?.FirstOrDefault(x => x.Value != null).Value;
            context.Items["User"] = claims!;
            request?.Identities.ToList().AddRange(claims?.Identities);

            await next(context);
        }
    }

    private bool HasAuthorizeEffect((AuthorizeAttribute? attribute, AttributeTargets attributeTargets, int prentLevel)? authorize,
        (AllowAnonymousAttribute? attribute, AttributeTargets attributeTargets, int prentLevel)? anonymous)
    {
        if (authorize is null)
            return false;

        if (anonymous is null)
            return true;

        if (authorize.GetValueOrDefault().attributeTargets == AttributeTargets.Class &&
            anonymous.GetValueOrDefault().attributeTargets == AttributeTargets.Method)
            return false;
        else if (authorize.GetValueOrDefault().attributeTargets == AttributeTargets.Method &&
            anonymous.GetValueOrDefault().attributeTargets == AttributeTargets.Class)
            return true;
        else if ((authorize.GetValueOrDefault().attributeTargets == AttributeTargets.Class &&
            anonymous.GetValueOrDefault().attributeTargets == AttributeTargets.Class) ||
            (authorize.GetValueOrDefault().attributeTargets == AttributeTargets.Method &&
            anonymous.GetValueOrDefault().attributeTargets == AttributeTargets.Method) &&
            (authorize.GetValueOrDefault().prentLevel <= anonymous.GetValueOrDefault().prentLevel))
            return true;

        if (authorize.GetValueOrDefault().prentLevel == anonymous.GetValueOrDefault().prentLevel)
        {
            if (authorize.GetValueOrDefault().attributeTargets == AttributeTargets.Method &&
                               anonymous.GetValueOrDefault().attributeTargets == AttributeTargets.Class)
                return true;
            else if (authorize.GetValueOrDefault().attributeTargets == AttributeTargets.Class &&
                               anonymous.GetValueOrDefault().attributeTargets == AttributeTargets.Method)
                return false;
            else
                return true;
        }
        else
        {
            return authorize.GetValueOrDefault().prentLevel < anonymous.GetValueOrDefault().prentLevel;
        }
    }

    private IEnumerable<string>? ParseSchemes(string? schemes)
    {
        if (schemes is null)
            return null;

        if (schemes.Contains(","))
            return schemes.Split(",");
        else
            return new List<string>() { schemes };
    }

    private IEnumerable<string>? ParseRoles(string roles)
    {
        if (roles is null)
            return null;

        if (roles.Contains(","))
            return roles.Split(",");
        else
            return new List<string>() { roles };
    }

    private bool UserIsInRole(ClaimsPrincipal user, IEnumerable<string> roles)
    {
        foreach (var role in roles)
        {
            if (user.IsInRole(role))
                return true;
        }

        return false;
    }
}
