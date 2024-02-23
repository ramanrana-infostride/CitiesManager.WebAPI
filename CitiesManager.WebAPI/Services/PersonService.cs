using System;
using System.Threading.Tasks;
using CitiesManager.Core.DTO;
using CitiesManager.Core.Entities;
using CitiesManager.Core.ServiceContracts;
using CitiesManager.Infrastructure.DatabaseContext;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

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

        public async Task<List<PersonDTO>> GetAllPersons()
        {
            var allPersons = await _context.Persons.ToListAsync();

            if (allPersons == null || !allPersons.Any())
            {
                return new List<PersonDTO>();
            }

            return allPersons.Select(person => new PersonDTO
            {
                Name = person.Name,
                Email = person.Email,
                Age = person.Age,
                PhoneNumber = person.PhoneNumber
            }).ToList();
        }

        public async Task<PersonDTO> GetPersonById (int id)
        {
            var person = await _context.Persons.FindAsync(id);

            if (person == null)
            {
                return null;
            }

            return new PersonDTO
            {
                Name = person.Name,
                Email = person.Email,
                Age = person.Age,
                PhoneNumber = person.PhoneNumber
            };
        }

        public async Task<PersonDTO> UpdatePerson (int id, PersonDTO updatedPerson)
        {
            var validationResult = await _personValidator.ValidateAsync(updatedPerson);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var existingPerson = await _context.Persons.FindAsync(id);

            if (existingPerson == null)
            {
                return null;
            }

            existingPerson.Name = updatedPerson.Name;
            existingPerson.Email = updatedPerson.Email;
            existingPerson.Age = updatedPerson.Age;
            existingPerson.PhoneNumber = updatedPerson.PhoneNumber;

            await _context.SaveChangesAsync();

            return new PersonDTO
            {
                Name = existingPerson.Name,
                Email = existingPerson.Email,
                Age = existingPerson.Age,
                PhoneNumber = existingPerson.PhoneNumber
            };
        }

        public async Task<PersonDTO> DeletePerson(int id)
        {
            var existingPerson = await _context.Persons.FindAsync(id);

            if (existingPerson != null)
            {
                _context.Persons.Remove(existingPerson);
                await _context.SaveChangesAsync();
                return new PersonDTO(); 
            }

            return null; 
        }

    }
}
