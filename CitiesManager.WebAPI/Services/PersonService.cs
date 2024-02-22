using System;
using System.Threading.Tasks;
using CitiesManager.Core.DTO;
using CitiesManager.Core.Entities;
using CitiesManager.Core.ServiceContracts;
using CitiesManager.Infrastructure.DatabaseContext;
using FluentValidation;

namespace CitiesManager.Core.Services
{
    public class PersonService : IPersonService
    {
        private readonly ApplicationDbContext _context;
        private readonly IValidator<PersonDTO> _personValidator;

        public PersonService(ApplicationDbContext context, IValidator<PersonDTO> personValidator)
        {
            _context = context;
            _personValidator = personValidator;
        }

        public async Task<PersonDTO> AddPerson(PersonDTO person)
        {
            var validationResult = await _personValidator.ValidateAsync(person);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var newPerson = new Person
            {
                Name = person.Name,
                Email = person.Email,
                Age = person.Age,
                PhoneNumber = person.PhoneNumber,
            };

            await _context.Persons.AddAsync(newPerson);
            await _context.SaveChangesAsync();

            var entity = new PersonDTO
            {
                Name = newPerson.Name,
                Email = newPerson.Email,
                Age = newPerson.Age,
                PhoneNumber = newPerson.PhoneNumber,
            };

            return entity;
        }
    }
}
