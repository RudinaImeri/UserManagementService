using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UserManagement.Domain.Common.Enums;
using UserManagement.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace UserManagement.Core.Context
{
    public static class Seed
    {
        private static ILoggers _log;

        public static void Initialize(ILoggers loggers)
        {
            _log = loggers;
        }
        public static void AddSeedData(this IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

                var dbFacade = context.GetDatabaseFacade();
                dbFacade.OpenConnection();

                var platformType = scope.ServiceProvider.GetRequiredService<PlatformType>();

                var platformEnvironment = scope.ServiceProvider.GetRequiredService<PlatformEnvironment>();


                var connection = dbFacade.GetDbConnection();
                using var command = connection.CreateCommand();
                command.CommandText = @"SELECT count(*) AS TOTALNUMBEROFTABLES
                                         FROM INFORMATION_SCHEMA.TABLES
                                         WHERE TABLE_SCHEMA = 'UserManagement'";
                command.CommandType = CommandType.Text;

                var result = command.ExecuteScalar();
                var tableCount = result != null ? (long?)Convert.ToInt64(result) : null;

                var appliedMigrations = dbFacade.GetAppliedMigrations();
                var pendingMigrations = dbFacade.GetPendingMigrations();

                var a = dbFacade.GetMigrations();
                dbFacade.Migrate();

                if (platformType == PlatformType.Web)
                {
                    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                    if (!userManager.Users.Any())
                    {
                        //admin admin
                        AddTestUsers(userManager, roleManager);
                    }

                    _ = context.SaveChangesAsync().Result;
                }
            }
        }

        private static async Task AddTestUsers(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            try
            {
                IdentityResult roleAddResult = new();

                var password = "Admin123.";
                var message = "";

                // Admin user
                var admin = new ApplicationUser
                {
                    FirstName = "Admin",
                    LastName = "Admin",
                    UserName = "admin",
                    Email = "admin@usermanagement.com",
                    PhoneNumber = "044111222",
                    Culture = "en",
                    Language = "en"
                };

                var identityResult = userManager.CreateAsync(admin, password).Result;
                var roleResult = roleManager.CreateAsync(new IdentityRole("Admin")).Result;
                admin = userManager.FindByNameAsync(admin.UserName).Result;
                if (admin != null)
                {
                    roleAddResult = userManager.AddToRoleAsync(admin, "Admin").Result;

                    message = "Test user and role: Admin was addedd successfully";
                    await _log.LogAsync(message, LogLevel.Information);
                }

                // Regular user
                var user = new ApplicationUser
                {
                    FirstName = "User",
                    LastName = "User",
                    UserName = "user",
                    Email = "user@usermanagement.com",
                    PhoneNumber = "044111222",
                    Culture = "en",
                    Language = "en"
                };
                identityResult = userManager.CreateAsync(user, password).Result;
                roleResult = roleManager.CreateAsync(new IdentityRole("User")).Result;
                user = userManager.FindByNameAsync(user.UserName).Result;
                if (user != null)
                {
                    roleAddResult = userManager.AddToRoleAsync(user, "User").Result;

                    message = "Test user and role: USer was addedd successfully";
                    await _log.LogAsync(message, LogLevel.Information);
                }
            }
            catch (Exception ex)
            {
                await _log.LogAsync(ex.Message, LogLevel.Error);
            }
        }
    }
}
