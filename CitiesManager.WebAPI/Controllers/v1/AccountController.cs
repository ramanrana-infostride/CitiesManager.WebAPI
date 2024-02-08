using CitiesManager.Core.DTO;
using CitiesManager.Core.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CitiesManager.WebAPI.Controllers.v1
{
    [AllowAnonymous]
    public class AccountController : CustomControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<ApplicationRole> roleManager)
        {
            _signInManager = signInManager;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<ActionResult<ApplicationUser>> PostRegister(RegisterDTO registerDTO)

        {
            ApplicationUser? userexists = await _userManager.FindByEmailAsync(registerDTO.Email);
            if (userexists != null)
            {
                return Conflict("User with this email already exists");
            }



            //Validation
            if (ModelState.IsValid == false)
            {
                String errorMessage = String.Join("|", ModelState.Values.SelectMany(v => v.Errors.Select(x => x.ErrorMessage)));
                return Problem(errorMessage);
            }

            //Create User

            ApplicationUser user = new ApplicationUser()
            {
                Email = registerDTO.Email,
                PhoneNumber = registerDTO.PhoneNumber,
                UserName = registerDTO.Email,
                PersonName = registerDTO.PersonName
            };

            IdentityResult result = await _userManager.CreateAsync(user, registerDTO.Password);

            if (result.Succeeded)
            {
                //sign -in 
                await _signInManager.SignInAsync(user, isPersistent: false);

                return Ok(user);
            }
            else
            {
                string errrorMessage = string.Join(" | ", result.Errors.Select(e => e.Description));
                return Problem(errrorMessage);
            }


        }



    }
}
