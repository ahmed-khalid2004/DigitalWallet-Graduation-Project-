using DigitalWallet.Domain.Entities;

namespace DigitalWallet.Application.Interfaces.Services
{
    /// <summary>
    /// Service for generating and validating JWT tokens
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Generates a JWT access token for the specified user
        /// </summary>
        string GenerateAccessToken(User user);

        /// <summary>
        /// Generates a refresh token
        /// </summary>
        string GenerateRefreshToken();

        /// <summary>
        /// Validates a refresh token and returns the user ID
        /// </summary>
        Guid ValidateRefreshToken(string refreshToken);
    }

    /// <summary>
    /// Service for password hashing and verification
    /// </summary>
}