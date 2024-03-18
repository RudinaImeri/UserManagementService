using System.ComponentModel.DataAnnotations;

namespace UserManagement.Domain.Dtos
{
    public class RegisterDto
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Firstname is required")]
        [StringLength(255, ErrorMessage = "Must be between 3 and 255 characters", MinimumLength = 3)]
        public string Firstname { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Lastname is required")]
        [StringLength(255, ErrorMessage = "Must be between 3 and 255 characters", MinimumLength = 3)]
        public string Lastname { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Username is required")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(255, ErrorMessage = "Must be between 5 and 255 characters", MinimumLength = 5)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm Password is required")]
        [StringLength(255, ErrorMessage = "Must be between 5 and 255 characters", MinimumLength = 5)]
        [DataType(DataType.Password)]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Phone Number is required")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Culture is required")]
        public string Culture { get; set; }

        [Required(ErrorMessage = "Language is required")]
        public string Language { get; set; }
    }
}
