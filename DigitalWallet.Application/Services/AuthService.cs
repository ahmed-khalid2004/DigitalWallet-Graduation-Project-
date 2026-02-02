using AutoMapper;
using DigitalWallet.Application.Common;
using DigitalWallet.Application.DTOs.Auth;
using DigitalWallet.Application.Interfaces.Repositories;
using DigitalWallet.Application.Interfaces.Services;
using DigitalWallet.Domain.Entities;
using DigitalWallet.Domain.Enums;
using DigitalWallet.Domain.Exceptions;
using System.Security.Cryptography;
using System.Text;

namespace DigitalWallet.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IWalletRepository _walletRepository;
        private readonly IOtpCodeRepository _otpRepository;
        private readonly IMapper _mapper;

        public AuthService(
            IUserRepository userRepository,
            IWalletRepository walletRepository,
            IOtpCodeRepository otpRepository,
            IMapper mapper)
        {
            _userRepository = userRepository;
            _walletRepository = walletRepository;
            _otpRepository = otpRepository;
            _mapper = mapper;
        }

        public async Task<ServiceResult<LoginResponseDto>> RegisterAsync(RegisterRequestDto request)
        {
            try
            {
                // Check if email exists
                if (await _userRepository.EmailExistsAsync(request.Email))
                    return ServiceResult<LoginResponseDto>.Failure("Email already exists");

                // Check if phone exists
                if (await _userRepository.PhoneExistsAsync(request.PhoneNumber))
                    return ServiceResult<LoginResponseDto>.Failure("Phone number already exists");

                // Hash password
                var (passwordHash, salt) = HashPassword(request.Password);

                // Create user
                var user = new User
                {
                    FullName = request.FullName,
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber,
                    PasswordHash = passwordHash,
                    Salt = salt,
                    KycLevel = KycLevel.Basic,
                    Status = UserStatus.Active
                };

                await _userRepository.AddAsync(user);

                // Create default wallet
                var wallet = new Wallet
                {
                    UserId = user.Id,
                    CurrencyCode = "EGP",
                    Balance = 0,
                    DailyLimit = 5000,
                    MonthlyLimit = 20000
                };

                await _walletRepository.AddAsync(wallet);

                // Generate token (simplified - you should use JWT)
                var response = new LoginResponseDto
                {
                    Token = GenerateToken(user.Id),
                    RefreshToken = GenerateRefreshToken(),
                    ExpiresAt = DateTime.UtcNow.AddHours(24),
                    UserId = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    RequiresOtp = false
                };

                return ServiceResult<LoginResponseDto>.Success(response, "Registration successful");
            }
            catch (Exception ex)
            {
                return ServiceResult<LoginResponseDto>.Failure($"Registration failed: {ex.Message}");
            }
        }

        public async Task<ServiceResult<LoginResponseDto>> LoginAsync(LoginRequestDto request)
        {
            try
            {
                // Find user by email or phone
                var user = await _userRepository.GetByEmailOrPhoneAsync(request.EmailOrPhone);

                if (user == null)
                    return ServiceResult<LoginResponseDto>.Failure("Invalid credentials");

                // Verify password
                if (!VerifyPassword(request.Password, user.PasswordHash, user.Salt))
                    return ServiceResult<LoginResponseDto>.Failure("Invalid credentials");

                // Check user status
                if (user.Status != UserStatus.Active)
                    return ServiceResult<LoginResponseDto>.Failure("Account is suspended or banned");

                // Update last login
                user.LastLoginAt = DateTime.UtcNow;
                await _userRepository.UpdateAsync(user);

                // Generate OTP for sensitive operations
                var otpCode = await GenerateOtpAsync(user.Id, "login");

                var response = new LoginResponseDto
                {
                    Token = GenerateToken(user.Id),
                    RefreshToken = GenerateRefreshToken(),
                    ExpiresAt = DateTime.UtcNow.AddHours(24),
                    UserId = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    RequiresOtp = true // For demonstration
                };

                return ServiceResult<LoginResponseDto>.Success(response, "Login successful");
            }
            catch (Exception ex)
            {
                return ServiceResult<LoginResponseDto>.Failure($"Login failed: {ex.Message}");
            }
        }

        public async Task<ServiceResult<bool>> VerifyOtpAsync(VerifyOtpRequestDto request)
        {
            try
            {
                var otp = await _otpRepository.GetValidOtpAsync(request.UserId, request.Code, OtpType.Login);

                if (otp == null)
                    return ServiceResult<bool>.Failure("Invalid or expired OTP");

                if (otp.IsUsed)
                    return ServiceResult<bool>.Failure("OTP already used");

                if (otp.ExpiresAt < DateTime.UtcNow)
                    return ServiceResult<bool>.Failure("OTP has expired");

                // Mark as used
                otp.IsUsed = true;
                await _otpRepository.UpdateAsync(otp);

                return ServiceResult<bool>.Success(true, "OTP verified successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Failure($"OTP verification failed: {ex.Message}");
            }
        }

        public async Task<ServiceResult<string>> GenerateOtpAsync(Guid userId, string type)
        {
            try
            {
                // Invalidate old OTPs
                await _otpRepository.InvalidateUserOtpsAsync(userId,
                    type == "login" ? OtpType.Login : OtpType.Transfer);

                // Generate 6-digit OTP
                var code = GenerateOtpCode();

                var otp = new OtpCode
                {
                    UserId = userId,
                    Code = code,
                    Type = type == "login" ? OtpType.Login : OtpType.Transfer,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                    IsUsed = false
                };

                await _otpRepository.AddAsync(otp);

                // In real app, send OTP via SMS/Email
                // For now, return it (ONLY for development)
                return ServiceResult<string>.Success(code, "OTP generated successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult<string>.Failure($"OTP generation failed: {ex.Message}");
            }
        }

        // Helper methods
        private (string hash, string salt) HashPassword(string password)
        {
            var saltBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(saltBytes);
            var salt = Convert.ToBase64String(saltBytes);

            var passwordBytes = Encoding.UTF8.GetBytes(password);
            var saltedPassword = new byte[saltBytes.Length + passwordBytes.Length];
            Buffer.BlockCopy(saltBytes, 0, saltedPassword, 0, saltBytes.Length);
            Buffer.BlockCopy(passwordBytes, 0, saltedPassword, saltBytes.Length, passwordBytes.Length);

            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(saltedPassword);
            var hash = Convert.ToBase64String(hashBytes);

            return (hash, salt);
        }

        private bool VerifyPassword(string password, string hash, string salt)
        {
            var saltBytes = Convert.FromBase64String(salt);
            var passwordBytes = Encoding.UTF8.GetBytes(password);
            var saltedPassword = new byte[saltBytes.Length + passwordBytes.Length];
            Buffer.BlockCopy(saltBytes, 0, saltedPassword, 0, saltBytes.Length);
            Buffer.BlockCopy(passwordBytes, 0, saltedPassword, saltBytes.Length, passwordBytes.Length);

            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(saltedPassword);
            var computedHash = Convert.ToBase64String(hashBytes);

            return computedHash == hash;
        }

        private string GenerateToken(Guid userId)
        {
            // Simplified - In production, use JWT
            return Convert.ToBase64String(Encoding.UTF8.GetBytes($"{userId}:{DateTime.UtcNow.Ticks}"));
        }

        private string GenerateRefreshToken()
        {
            var randomBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        private string GenerateOtpCode()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }
    }
}