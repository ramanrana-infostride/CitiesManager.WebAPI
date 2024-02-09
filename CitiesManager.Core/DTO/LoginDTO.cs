using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CitiesManager.Core.DTO
{
    public class LoginDTO
    {
        [Required(ErrorMessage = "Error Message Cant be blank")]
        [EmailAddress(ErrorMessage = "Email should be in a proper format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password cant be blank")]
        public string Password { get; set; }
    }
}
