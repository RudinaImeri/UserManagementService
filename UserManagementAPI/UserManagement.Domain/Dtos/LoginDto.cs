using System;
using System.ComponentModel.DataAnnotations;

namespace UserManagement.Domain.Dtos
{
    public class LoginDto
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Username must fill")]
        [StringLength(255, ErrorMessage = "Must be between 5 and 255 characters", MinimumLength = 5)]
        public string Username { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Password must fill")]
        [StringLength(255, ErrorMessage = "Must be between 5 and 255 characters", MinimumLength = 5)]
        public string Password { get; set; }
    }
}
