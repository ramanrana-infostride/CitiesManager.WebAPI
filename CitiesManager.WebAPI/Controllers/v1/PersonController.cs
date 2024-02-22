using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using CitiesManager.Core.ServiceContracts;
using CitiesManager.Core.DTO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace CitiesManager.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class PersonController : ControllerBase
    {
        private readonly IValidator<PersonDTO> _validator;
        private readonly IPersonService _personService;

        public PersonController(IPersonService personService, IValidator<PersonDTO> validator)
        {
            _personService = personService;
            _validator = validator;
        }

        [HttpPost("add-person")]
        public async Task<IActionResult> AddPerson (PersonDTO personDTO)
        {
            var validationResult = await _validator.ValidateAsync(personDTO);

            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var entity = await _personService.AddPerson(personDTO);

            if (entity == null)
            {
                return StatusCode(500, "Failed to add person");
            }

            return Ok(entity);
        }
    }
}
