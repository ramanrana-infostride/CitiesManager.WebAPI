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

        [HttpGet("get-all-persons")]
        public async Task<IActionResult> GetAllPersons()
        {
            var persons = await _personService.GetAllPersons();

            if (persons == null || !persons.Any())
            {
                return NotFound("No persons found.");
            }

            return Ok(persons);
        }

        [HttpGet("get-person/{id}")]
        public async Task<IActionResult> GetPerson(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid person ID.");
            }

            var person = await _personService.GetPersonById(id);

            if (person == null)
            {
                return NotFound();
            }

            return Ok(person);
        }

        [HttpPut("update-person/{id}")]
        public async Task<IActionResult> UpdatePerson(int id, PersonDTO updatedPerson)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid person ID.");
            }

            var validationResult = await _validator.ValidateAsync(updatedPerson);

            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var entity = await _personService.UpdatePerson(id, updatedPerson);

            if (entity == null)
            {
                return NotFound();
            }

            return Ok(entity);
        }

        [HttpDelete("delete-person/{id}")]
        public async Task<IActionResult> DeletePerson(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid person ID.");
            }

            await _personService.DeletePerson(id);

            return NoContent();
        }


    }
}
