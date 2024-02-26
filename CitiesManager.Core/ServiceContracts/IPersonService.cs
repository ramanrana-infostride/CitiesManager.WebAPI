using CitiesManager.Core.DTO;
using CitiesManager.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CitiesManager.Core.ServiceContracts
{
    public interface IPersonService
    {
        Task<PersonDTO> AddPerson (PersonDTO person);
        Task<List<PersonDTO>> GetAllPersons();
        Task<PersonDTO> GetPersonById(int id);
        Task<PersonDTO> UpdatePerson(int id, PersonDTO updatedPerson);
        Task<PersonDTO> DeletePerson (int id);
    }
}
