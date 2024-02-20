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

    }
}
