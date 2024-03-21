using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagement.Core.Repositories;
using UserManagement.Domain.Dtos;
using UserManagement.Domain.Entities;

namespace UserManagement.Core.Service
{
    public interface IUserService
    {
        public Task DeleteUserAsync(ApplicationUser user);
        public Task<bool> ValidateUserPasswordAsync(string userName, string password);
    }
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task DeleteUserAsync(ApplicationUser user)
        {
            await _userRepository.DeleteUserAsync(user);
        }

        public Task<bool> ValidateUserPasswordAsync(string userName, string password)
        {
            return _userRepository.ValidateUserPasswordAsync(userName, password);
        }
    }
}
