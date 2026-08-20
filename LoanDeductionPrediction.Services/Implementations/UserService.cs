using AutoMapper;
using BCrypt.Net;
using LoanDeductionPrediction.Models.DTOs;
using LoanDeductionPrediction.Repositories.Entities;
using LoanDeductionPrediction.Repositories.Interfaces;
using LoanDeductionPrediction.Services.Interfaces;

namespace LoanDeductionPrediction.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UserService(
            IUserRepository userRepository,
            IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<User?> AuthenticateAsync(
            string email,
            string password)
        {
            var user =
                await _userRepository.GetByEmailAsync(email);

            if (user == null || !user.IsActive)
            {
                return null;
            }

            bool passwordValid;

            try
            {
                passwordValid =
                    BCrypt.Net.BCrypt.Verify(
                        password,
                        user.PasswordHash);
            }
            catch
            {
                passwordValid = false;
            }

            if (!passwordValid)
            {
                return null;
            }

            return user;
        }

        public async Task<UserDto?> GetByIdAsync(
            int userId)
        {
            var user =
                await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                return null;
            }

            return _mapper.Map<UserDto>(user);
        }

        public async Task<List<UserDto>> GetAllAsync()
        {
            var users =
                await _userRepository.GetAllAsync();

            return _mapper.Map<List<UserDto>>(users);
        }
    }
}