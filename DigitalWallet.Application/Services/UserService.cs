using AutoMapper;
using DigitalWallet.Application.Common;
using DigitalWallet.Application.DTOs.Auth;
using DigitalWallet.Application.Interfaces.Repositories;
using DigitalWallet.Application.Interfaces.Services;

namespace DigitalWallet.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UserService(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<ServiceResult<UserDto>> GetUserByIdAsync(Guid userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);

                if (user == null)
                    return ServiceResult<UserDto>.Failure("User not found");

                var userDto = _mapper.Map<UserDto>(user);
                return ServiceResult<UserDto>.Success(userDto);
            }
            catch (Exception ex)
            {
                return ServiceResult<UserDto>.Failure($"Error retrieving user: {ex.Message}");
            }
        }

        public async Task<ServiceResult<UserDto>> GetUserByEmailAsync(string email)
        {
            try
            {
                var user = await _userRepository.GetByEmailAsync(email);

                if (user == null)
                    return ServiceResult<UserDto>.Failure("User not found");

                var userDto = _mapper.Map<UserDto>(user);
                return ServiceResult<UserDto>.Success(userDto);
            }
            catch (Exception ex)
            {
                return ServiceResult<UserDto>.Failure($"Error retrieving user: {ex.Message}");
            }
        }
    }
}