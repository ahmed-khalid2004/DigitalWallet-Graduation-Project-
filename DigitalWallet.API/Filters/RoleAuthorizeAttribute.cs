using DigitalWallet.Application.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public sealed class RoleAuthorizeAttribute : Attribute, IFilterFactory
{
    public string[] Roles { get; }

    public RoleAuthorizeAttribute(params string[] roles)
    {
        Roles = roles;
    }

    public bool IsReusable => true;

    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
        return new AuthorizationFilter(
            Roles,
            serviceProvider.GetRequiredService<ILogger<AuthorizationFilter>>()
        );
    }
}


// ═════════════════════════════════════════════════════════════════════════
// 2.  Filter  –  the actual logic executed for every request that has the
//     attribute applied.
// ═════════════════════════════════════════════════════════════════════════

/// <summary>
/// Checks that the request is authenticated AND (optionally) that the principal
/// holds at least one of the roles specified by <see cref="RoleAuthorizeAttribute"/>.
///
/// Response contract:
///   • Not authenticated          → 401  with ApiResponse error message
///   • Authenticated but no role  → 403  with ApiResponse error message
///   • All checks pass            → pipeline continues normally
/// </summary>
public class AuthorizationFilter : IActionFilter
{
    private readonly string[] _roles;
    private readonly ILogger<AuthorizationFilter> _logger;

    public AuthorizationFilter(string[] roles, ILogger<AuthorizationFilter> logger)
    {
        _roles = roles;
        _logger = logger;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        var user = context.HttpContext.User;

        if (user == null || !user.Identity!.IsAuthenticated)
        {
            context.Result = new UnauthorizedObjectResult(
                ApiResponse<object>.ErrorResponse("Authentication is required."));
            return;
        }

        if (_roles.Length == 0)
            return;

        var userRoles = user.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (!_roles.Any(r => userRoles.Contains(r)))
        {
            context.Result = new ObjectResult(
                ApiResponse<object>.ErrorResponse(
                    $"Access denied. Required role(s): {string.Join(", ", _roles)}."))
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}