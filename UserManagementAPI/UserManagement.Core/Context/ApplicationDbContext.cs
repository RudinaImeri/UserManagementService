using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using UserManagement.Domain.Entities;

namespace UserManagement.Core.Context
{
    public interface IApplicationDbContext
    {
        public Task<int> SaveChangesAsync();
        public DatabaseFacade GetDatabaseFacade();
        
    }
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            this.Database.SetCommandTimeout(TimeSpan.FromSeconds(30));
        }

        public ApplicationDbContext()
        {
        }

        public async Task<int> SaveChangesAsync()
        {
            return await base.SaveChangesAsync();
        }

        public DatabaseFacade GetDatabaseFacade()
        {
            var bb = this.Database.GetDbConnection().ConnectionString;
            var aa = this.Database.CanConnect();

            return this.Database;
        }
    }
}
