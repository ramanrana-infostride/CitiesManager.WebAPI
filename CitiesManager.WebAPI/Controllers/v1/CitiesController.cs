using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CitiesManager.Infrastructure.DatabaseContext;
using CitiesManager.Core.Models;
using Microsoft.AspNetCore.Cors;


namespace CitiesManager.WebAPI.Controllers.V1
{
    [ApiVersion("1.0")]
    //   [EnableCors("4100Client")]
    public class CitiesController : CustomControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CitiesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Cities
        /// <summary>
        /// To get List Of cities including City ID and City Name
        /// </summary>
        /// <returns></returns>

        //This method fetches a list of cities using OrderBy, sorting them alphabetically by their names.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<City>>> GetCities()
        {
            var cities = await _context.Cities.OrderBy(temp => temp.CityName).ToListAsync();
            return cities;
        }

        // GET: api/Cities/5
        [HttpGet("{cityID}")]
        public async Task<ActionResult<City>> GetCity(Guid cityId)
        {
            var city = await _context.Cities.FirstOrDefaultAsync(temp => temp.CityId == cityId);

            if (city == null)
            {
                return Problem(detail: "Invalid City Id", statusCode: 400, title: "City Search");
                // return NotFound();
            }
            return city;
        }

        // PUT: api/Cities/5
        [HttpPut("{cityID}")]
        public async Task<IActionResult> PutCity(Guid cityID, [Bind(nameof(City.CityId), nameof(City.CityName))] City city)
        {
            if (cityID != city.CityId)
            {
                return BadRequest(); //HTTP 400
            }

            var existingCity = await _context.Cities.FindAsync(cityID);
            if (existingCity == null)
            {
                return NotFound(); //HTTP 404
            }

            existingCity.CityName = city.CityName;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CityExists(cityID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return NoContent();
        }


        // POST: api/Cities
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<City>> PostCity([Bind(nameof(City.CityId), nameof(City.CityName))] City city)
        {
            if (_context.Cities == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Cities'  is null.");
            }
            _context.Cities.Add(city);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCity", new { cityID = city.CityId }, city); //api/Cities/67d28f3d-43eb-49c7-916c-5b39172955e5
        }

      
        // DELETE: api/Cities/5
        [HttpDelete]
        public async Task<IActionResult> DeleteCity(Guid CityId)
        {
            var city = await _context.Cities.FindAsync(CityId);
            if (city == null)
            {
                return NotFound(); //HTTP 404
            }

            _context.Cities.Remove(city);
            await _context.SaveChangesAsync();

            return NoContent(); //HTTP 200
        }

        private bool CityExists(Guid CityId)
        {
            return (_context.Cities?.Any(e => e.CityId == CityId)).GetValueOrDefault();
        }
    }
}
