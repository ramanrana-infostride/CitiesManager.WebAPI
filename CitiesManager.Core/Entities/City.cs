using System.ComponentModel.DataAnnotations;

namespace CitiesManager.Core.Models
{
    public class City
    {
        [Key]
        public Guid CityId { get; set; }

        [Required(ErrorMessage = "City name Cannot be blank")]
        public string? CityName { get; set; }
    }
}
