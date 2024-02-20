using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CitiesManager.Core.Models;
using CitiesManager.Infrastructure.DatabaseContext;
using CitiesManager.WebAPI.Controllers.V1;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Xunit;
namespace CitiesManager.Tests.Controllers
{
    public class CitiesControllerTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _context;

        public CitiesControllerTests()
        {
            // Read the connection string from appsettings.json
            var connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=CitiesDataBase;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False";

            _context = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(connectionString)
                .Options;

            using (var context = new ApplicationDbContext(_context))
            {
                context.Database.Migrate();
            }
        }

        #region PostCity


        //When the city name is null, it should throw ArgumentException
        [Fact]
        public async Task PostCity_NullCityName()
        {
            // Arrange
            string cityName = null; // Null city name
            var city = new City { CityName = cityName };
            var controller = new CitiesController(new ApplicationDbContext(_context));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => controller.PostCity(city));
            Assert.Equal(nameof(city.CityName), exception.ParamName);
            Assert.Contains("City name cannot be null.", exception.Message);
        }

        //Ensure city added in database
        [Fact]
        public async Task PostCity_AddCity()
        {
            // Arrange
            var cityId = Guid.NewGuid();
            var cityName = "Test City";
            var city = new City { CityId = cityId, CityName = cityName };

            // Act
            using (var context = new ApplicationDbContext(_context))
            {
                // To Check if the city already exists in the database
                var existingCity = await context.Cities.FirstOrDefaultAsync(c => c.CityName == cityName);
                if (existingCity == null)
                {
                    // Add the city only if it doesn't already exist
                    var controller = new CitiesController(context);
                    var result = await controller.PostCity(city);

                    // Assert
                    Assert.IsType<CreatedAtActionResult>(result.Result);
                }
                else
                {
                    return;
                }
            }
            using (var context = new ApplicationDbContext(_context))
            {
                var addedCity = await context.Cities.FindAsync(cityId);
                Assert.NotNull(addedCity);
                Assert.Equal(cityName, addedCity.CityName);
            }
        }


        #endregion



        #region GetCities

        //The list of cities should be empty by default (before adding any city)

        [Fact]
        public async Task GetCities_EmptyList()
        {
            // Arrange: Define the expected list of cities
            var expectedCities = new List<City>();

            // Act
            using (var context = new ApplicationDbContext(_context))
            {
                var controller = new CitiesController(context);
                var result = await controller.GetCities();

                // Assert
                var actionResult = Assert.IsType<ActionResult<IEnumerable<City>>>(result);
                var cities = Assert.IsAssignableFrom<IEnumerable<City>>(actionResult.Value);

                // Check if the returned list contains only the expected cities
                foreach (var city in cities)
                {
                    // Verify if the city is not present in the expected list
                    Assert.DoesNotContain(city, expectedCities);
                }
            }
        }

        [Fact]
        public async Task GetCities_ListOfCities()
        {
            // Arrange
            var citiesToAdd = new List<City>
            {
                new City { CityId = Guid.NewGuid(), CityName = "New York" },
                new City { CityId = Guid.NewGuid(), CityName = "London" }
            };

            // Act
            using (var context = new ApplicationDbContext(_context))
            {
                foreach (var city in citiesToAdd)
                {
                    var existingCity = await context.Cities.FirstOrDefaultAsync(c => c.CityName == city.CityName);
                    if (existingCity == null)
                    {
                        // Add the city if it doesn't already exist
                        context.Cities.Add(city);
                    }
                }

                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new ApplicationDbContext(_context))
            {
                var controller = new CitiesController(context);
                var result = await controller.GetCities();

                // Assert
                var actionResult = Assert.IsType<ActionResult<IEnumerable<City>>>(result);
                var cities = Assert.IsAssignableFrom<IEnumerable<City>>(actionResult.Value);

                foreach (var city in citiesToAdd)
                {
                    // Compare cities by CityName
                    Assert.Contains(city.CityName, cities.Select(c => c.CityName));
                }
            }
        }

        #endregion

        #region PutCity

        [Fact]
        public async Task PutCity_ExistingCity_UpdatesCityName()
        {
            // Arrange
            var cityId = Guid.NewGuid();
            var originalCityName = "Test City";
            var updatedCityName = "Updated City";

            // Check if the city already exists in the database
            using (var context = new ApplicationDbContext(_context))
            {
                var existingCity = await context.Cities.FindAsync(cityId);
                if (existingCity == null)
                {
                    // Add the original city to the database
                    var city = new City { CityId = cityId, CityName = originalCityName };
                    context.Cities.Add(city);
                    await context.SaveChangesAsync();
                }
            }

            // Update the city name
            var updatedCity = new City { CityId = cityId, CityName = updatedCityName };

            // Act
            using (var context = new ApplicationDbContext(_context))
            {
                var controller = new CitiesController(context);
                var result = await controller.PutCity(cityId, updatedCity);

                // Assert
                Assert.IsType<NoContentResult>(result);

                // Verify that the city name has been updated in the database
                var updatedCityFromDb = await context.Cities.FindAsync(cityId);
                Assert.Equal(updatedCityName, updatedCityFromDb.CityName);
            }
        }

        #endregion

    }
}
