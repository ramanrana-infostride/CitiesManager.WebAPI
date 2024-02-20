using CitiesManager.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CitiesManager.WebAPI.Controllers.v1
{
    [Authorize]
    [ApiVersion("1.0")]
    public class AdminController : CustomControllerBase

    {
        [HttpGet("employees")]
        public IEnumerable<string> GetEmployees()
        {
            return new List<string> { "ACvvv", "sfsdfsdf", "adasdasd" };
        }

    }
}
