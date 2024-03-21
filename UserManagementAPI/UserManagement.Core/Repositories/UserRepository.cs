using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagement.Core.Context;
using UserManagement.Core.Service;
using UserManagement.Core.Utilities;
using UserManagement.Domain.Dtos;
using UserManagement.Domain.Entities;

namespace UserManagement.Core.Repositories
{
    public interface IUserRepository
    {
        public Task DeleteUserAsync(ApplicationUser user);
        public Task<bool> ValidateUserPasswordAsync(string userName, string password);
    }
    public class UserRepository : IUserRepository
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapperService _mapperService;
        private UserManager<ApplicationUser> _userManager;


        public UserRepository(IApplicationDbContext context, IMapperService mapperService, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _mapperService = mapperService;
            _userManager = userManager;
        }

        public async Task DeleteUserAsync(ApplicationUser user)
        {
            await _userManager.DeleteAsync(user);
        }

        public async Task<bool> ValidateUserPasswordAsync(string userName, string password)
        {
            return true;
        }
    }
}
