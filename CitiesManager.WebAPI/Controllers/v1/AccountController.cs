using CitiesManager.Core.DTO;
using CitiesManager.Core.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CitiesManager.WebAPI.Controllers.v1
{
    /// <summary>
    /// 
    /// </summary>
    [AllowAnonymous]
    [ApiVersion("1.0")]
    public class AccountController : CustomControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userManager"></param>
        /// <param name="signInManager"></param>
        /// <param name="roleManager"></param>
        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<ApplicationRole> roleManager)
        {
            _signInManager = signInManager;
            _roleManager = roleManager;
            _userManager = userManager;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="registerDTO"></param>
        /// <returns></returns>
        [HttpPost("register")]
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

        [HttpPost("login")]
        public async Task<ActionResult<ApplicationUser>> PostLogin(LoginDTO loginDTO)
        {
            //Validation
            if (ModelState.IsValid == false)
            {
                String errorMessage = String.Join("|", ModelState.Values.SelectMany(v => v.Errors.Select(x => x.ErrorMessage)));
                return Problem(errorMessage);
            }

            var result = await _signInManager.PasswordSignInAsync(loginDTO.Email, loginDTO.Password, isPersistent: false, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                ApplicationUser? user = await _userManager.FindByEmailAsync(loginDTO.Email);
                if (user == null) { return NoContent(); }
                return Ok(new { personName = user.PersonName, email = user.Email });

            }
            else
            {
                return Problem("Invalid name or password");
            }

        }


        [HttpGet("logout")]
        public async Task<IActionResult> GetLogout()

        {
            await _signInManager.SignOutAsync();

            return NoContent();
        }



    }
}
