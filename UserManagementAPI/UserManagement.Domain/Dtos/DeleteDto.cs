using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManagement.Domain.Dtos
{
    public class DeleteDto
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Username must fill")]
       public string Username { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Password must fill")]
         public string Password { get; set; }
    }
}
