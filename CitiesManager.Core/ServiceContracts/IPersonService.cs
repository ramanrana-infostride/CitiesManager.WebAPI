using CitiesManager.Core.DTO;
using CitiesManager.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CitiesManager.Core.ServiceContracts
{
    public interface IPersonService
    {
        Task<PersonDTO> AddPerson (PersonDTO person);

    }
}
