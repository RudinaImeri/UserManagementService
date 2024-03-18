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

namespace UserManagement.Core.Context
{
    public static class Seed
    {
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


                //var aa = dbFacade.EnsureDeleted();

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

                //TODO
                //if (platformEnvironment == PlatformEnvironment.Local)
                {
                    //AddTickets(ticketService);
                }
            }
        }

        private static async Task AddTestUsers(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            var password = "Admin123.";

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
            try
            {

            var identityResult = userManager.CreateAsync(admin, password).Result;
            var roleResult = roleManager.CreateAsync(new IdentityRole("Admin")).Result;
            admin = userManager.FindByNameAsync(admin.UserName).Result;
            var roleAddResult = userManager.AddToRoleAsync(admin, "Admin").Result;
            // Manager user
           
            var manager = new ApplicationUser
            {
                FirstName = "Manager",
                LastName = "Manager",
                UserName = "manager",
                Email = "manager@usermanagement.com",
                PhoneNumber = "044111222",
                Culture = "en",
                Language = "en"
            };
            identityResult = userManager.CreateAsync(manager, password).Result;
            roleResult = roleManager.CreateAsync(new IdentityRole("Manager")).Result;
            manager = userManager.FindByNameAsync(manager.UserName).Result;
            roleAddResult = userManager.AddToRoleAsync(manager, "Manager").Result;

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
            roleAddResult = userManager.AddToRoleAsync(user, "User").Result;
            }
            catch (Exception ex)
            {
                var test = ex.Message;
            }
        }

        private static async Task CreateUserAndAddToRoleAsync(UserManager<ApplicationUser> userManager, ApplicationUser user, string password, string roleName)
        {
            var userExists = await userManager.FindByNameAsync(user.UserName);
            if (userExists == null)
            {
                try
                {

                var identityResult = userManager.CreateAsync(user, password).Result;
                if (identityResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, roleName);
                }
                else
                {
                    // Handle error, e.g., log or throw
                }
                }
                catch(Exception ex)
                {
                    var test = ex.Message;
                }
            }
        }

    }
}
