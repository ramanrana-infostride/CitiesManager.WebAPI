using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CitiesManager.Core.DTO
{
    public class RegisterDTO
    {
        [Required(ErrorMessage = "Person Name Cant be blank")]
        public string PersonName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email Name Cant be blank")]
        [EmailAddress(ErrorMessage = "Email Should be in proper format")]
        public string? Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email Name Cant be blank")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Phone Number Should be in proper format")]
        public string? PhoneNumber { get; set; } = string.Empty;


        [Required(ErrorMessage = "Password Name Cant be blank")]

        public string? Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm Password Cant be blank")]
        [Compare("Password", ErrorMessage = "Confirm Password and password do not match")]

        public string? ConfirmPassword { get; set; } = string.Empty;


    }
}
