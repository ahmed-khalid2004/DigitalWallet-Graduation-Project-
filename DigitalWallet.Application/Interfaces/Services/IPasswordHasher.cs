using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalWallet.Application.Interfaces.Services
{
    public interface IPasswordHasher
    {
        /// <summary>
        /// Hashes a password with the provided salt
        /// </summary>
        string HashPassword(string password, string salt);

        /// <summary>
        /// Generates a cryptographic salt
        /// </summary>
        string GenerateSalt();

        /// <summary>
        /// Verifies a password against a hash
        /// </summary>
        bool VerifyPassword(string password, string hash, string salt);
    }
}
