using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;


namespace UserManagement.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        [MaxLength(30)]
        public string FirstName { get; set; }

        [MaxLength(30)]
        public string LastName { get; set; }

        [MaxLength(50)]
        public string Language { get; set; }

        [MaxLength(50)]
        public string Culture { get; set; }
    }
}
