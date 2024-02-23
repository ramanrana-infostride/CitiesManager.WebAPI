using Azure;
using CitiesManager.Core.DTO;
using CitiesManager.Core.Identity;
using CitiesManager.Core.Models;
using CitiesManager.Core.ServiceContracts;
using CitiesManager.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using NuGet.Common;
using System.Security.Claims;

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
        private readonly IJwtService _jwtService;
        private readonly IEmailServices _emailservice;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userManager"></param>
        /// <param name="signInManager"></param>
        /// <param name="roleManager"></param>
        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<ApplicationRole> roleManager, IJwtService jwtService, IEmailServices emailservice)
        {
            _signInManager = signInManager;
            _roleManager = roleManager;
            _userManager = userManager;
            _jwtService = jwtService;
            _emailservice = emailservice;
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
                PersonName = registerDTO.PersonName,
                EmailConfirmed = false,
                TwoFactorEnabled = true,
            };

            IdentityResult result = await _userManager.CreateAsync(user, registerDTO.Password);

            if (result.Succeeded)
            {


                var verificationLink = Url.Action("VerifyEmail", "Account", new { userId = user.Id }, Request.Scheme);

                // Send verification email
                await SendVerificationEmailAsync(user.Email, verificationLink);


                await _userManager.AddToRolesAsync(user, new[] { "User" });

                //sign -in 
                await _signInManager.SignInAsync(user, isPersistent: false);
                var authenticationResponse = _jwtService.CreateJwtToken(user);

                user.RefreshToken = authenticationResponse.RefreshToken;
                user.RefreshTokenExpirationDateTime = authenticationResponse.RefreshTokenExpirationDateTime;
                await _userManager.UpdateAsync(user);



                return Ok(authenticationResponse);
            }
            else
            {
                string errrorMessage = string.Join(" | ", result.Errors.Select(e => e.Description));
                return Problem(errrorMessage);
            }


        }


        [HttpGet("VerifyEmail")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string userId)
        {
            if (userId == null)
            {
                return BadRequest("Invalid token or user ID");
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {

                return BadRequest("User not found");
            }

            if (user.EmailConfirmed == true)
            {

                return BadRequest("User Already Verified");
            }
            user.EmailConfirmed = true;
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return Ok("Email verification successful");
            }
            else
            {
                // Handle the case where updating the user fails
                return Problem("Failed to verify email");
            }

        }





        [HttpPost]
        public IActionResult SendEmail(EmailDto request)
        {
            _emailservice.SendEmail(request);

            return Ok();
        }

        private async Task SendVerificationEmailAsync(string userEmail, string verificationLink)
        {
            var emailDto = new EmailDto
            {
                To = userEmail,
                Subject = "Verify your email address",
                Body = $"<p>Thank you for registering with Your Application. Please verify your email address by clicking the following link:</p><p><a href='{verificationLink}'>Verify Email</a></p>"
            };

            _emailservice.SendEmail(emailDto);
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
            /*     ApplicationUser? user = await _userManager.FindByEmailAsync(loginDTO.Email);
                 if (user.TwoFactorEnabled == true)
                 {

                     var token = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");

                     var email = new EmailDto
                     {
                         To = user.Email,
                         Subject = "Otp to Login",
                         Body = $"<p>{token}</p>"
                     };

                     _emailservice.SendEmail(email);

                     return Ok($"We have shared an OTP to your Email {user.Email}");
                 }*/
            if (result.Succeeded)
            {
                ApplicationUser? user = await _userManager.FindByEmailAsync(loginDTO.Email);
                if (user == null)
                {
                    return NoContent();
                }



                var authenticationResponse = _jwtService.CreateJwtToken(user);
                user.RefreshToken = authenticationResponse.RefreshToken;
                user.RefreshTokenExpirationDateTime = authenticationResponse.RefreshTokenExpirationDateTime;
                await _userManager.UpdateAsync(user);
                return Ok(authenticationResponse);


            }
            else
            {
                return Problem("Invalid name or password");
            }



        }


        /*        [HttpPost("login-2FA")]
                public async Task<ActionResult<ApplicationUser>> PostLogin2FA(string code,string useremail)
                {
                    ApplicationUser? user = await _userManager.FindByEmailAsync(useremail);
                    var signIn =  _signInManager.ValidateTwoFactorSecurityStampAsync("Email", code, false, false);

                    if (signIn.IsCompletedSuccessfully)
                    {
                       // ApplicationUser? user = await _userManager.FindByEmailAsync(useremail);
                        if (user == null)
                        {
                            return NoContent();
                        }
                        var authenticationResponse = _jwtService.CreateJwtToken(user);
                        user.RefreshToken = authenticationResponse.RefreshToken;
                        user.RefreshTokenExpirationDateTime = authenticationResponse.RefreshTokenExpirationDateTime;
                        await _userManager.UpdateAsync(user);
                        return Ok(authenticationResponse);
                    }
                    else
                    {
                        return Problem("Invalid name or password");
                    }



                }*/


        [HttpGet("logout")]
        public async Task<IActionResult> GetLogout()

        {
            await _signInManager.SignOutAsync();

            return NoContent();
        }

        [HttpPost("generate-new-jwt-token")]
        public async Task<IActionResult> GenerateNewAccessToken(TokenModel tokenModel)
        {
            if (tokenModel == null)
            {
                return BadRequest("Invalid Client Request");
            }

            string? jwtToken = tokenModel.Token;
            string? refreshToken = tokenModel.RefreshToken;

            ClaimsPrincipal? principal = _jwtService.GetPrincipalFromJwtToken(tokenModel.Token);

            if (principal == null) { return BadRequest("Invalid Jwt access Token"); };

            string? email = principal.FindFirstValue(ClaimTypes.Email);

            ApplicationUser? user = await _userManager.FindByNameAsync(email);

            if (user == null || user.RefreshToken != tokenModel.RefreshToken || user.RefreshTokenExpirationDateTime <= DateTime.Now) { return BadRequest("Invalid Refresh Token"); }

            AuthenticationResponse authenticationResponse = _jwtService.CreateJwtToken(user);

            user.RefreshToken = authenticationResponse.RefreshToken;
            user.RefreshTokenExpirationDateTime = authenticationResponse.RefreshTokenExpirationDateTime;

            await _userManager.UpdateAsync(user);

            return Ok(authenticationResponse);
        }

    }
}
