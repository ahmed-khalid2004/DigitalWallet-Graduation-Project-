using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace DigitalWallet.API.Middleware
{
    /// <summary>
    /// Custom JWT authentication middleware that validates Bearer tokens and populates
    /// the HttpContext.User principal with claims.
    /// 
    /// Flow:
    /// 1. Extracts token from Authorization header (format: "Bearer {token}")
    /// 2. Validates token signature, issuer, audience, and expiration
    /// 3. On success: Sets HttpContext.User with claims from token
    /// 4. On failure: Continues pipeline with unauthenticated user (controllers handle 401)
    /// 
    /// This middleware runs before UseAuthorization() so that [Authorize] attributes
    /// can access the authenticated principal.
    /// </summary>
    public class JwtAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private readonly ILogger<JwtAuthenticationMiddleware> _logger;

        // JWT configuration from appsettings.json
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;

        public JwtAuthenticationMiddleware(
            RequestDelegate next,
            IConfiguration configuration,
            ILogger<JwtAuthenticationMiddleware> logger)
        {
            _next = next;
            _configuration = configuration;
            _logger = logger;

            _secretKey = _configuration["Jwt:SecretKey"]
                ?? throw new InvalidOperationException("JWT SecretKey is not configured");
            _issuer = _configuration["Jwt:Issuer"] ?? "DigitalWallet.API";
            _audience = _configuration["Jwt:Audience"] ?? "DigitalWallet.Clients";
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // ── 1. Extract token from Authorization header ────────────────
            var authHeader = context.Request.Headers.Authorization.ToString();

            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var token = authHeader.Substring("Bearer ".Length).Trim();

                // ── 2. Validate token ──────────────────────────────────────
                var principal = ValidateToken(token);

                if (principal != null)
                {
                    // ── 3. Set authenticated user ──────────────────────────
                    context.User = principal;

                    var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    var role = principal.FindFirst(ClaimTypes.Role)?.Value;

                    _logger.LogDebug("JWT Authentication successful: UserId={UserId}, Role={Role}", userId, role);
                }
                else
                {
                    _logger.LogWarning("JWT Authentication failed: Invalid token");
                }
            }

            // ── 4. Continue pipeline ───────────────────────────────────────
            // Even if authentication fails, we continue the pipeline.
            // Controllers with [Authorize] will return 401 if User is not authenticated.
            await _next(context);
        }

        /// <summary>
        /// Validates JWT token and returns ClaimsPrincipal if valid, null otherwise
        /// </summary>
        private ClaimsPrincipal? ValidateToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_secretKey);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _issuer,
                    ValidateAudience = true,
                    ValidAudience = _audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero // No tolerance for expired tokens
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

                // Additional validation: ensure it's a JWT token
                if (validatedToken is not JwtSecurityToken jwtToken ||
                    !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    _logger.LogWarning("Token validation failed: Invalid algorithm");
                    return null;
                }

                return principal;
            }
            catch (SecurityTokenExpiredException)
            {
                _logger.LogDebug("Token validation failed: Token expired");
                return null;
            }
            catch (SecurityTokenInvalidSignatureException)
            {
                _logger.LogWarning("Token validation failed: Invalid signature");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Token validation failed: {Message}", ex.Message);
                return null;
            }
        }
    }
}