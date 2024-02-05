﻿using CitiesManager.WebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CitiesManager.WebAPI.DatabaseContext
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public ApplicationDbContext() { }

        public virtual DbSet<City> Cities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<City>().HasData(new City()
            {
                CityId = Guid.Parse("5458405B-0DA0-4E1C-8DCC-EF07A7C33D83"),
                CityName = "Mohali"
            });

            modelBuilder.Entity<City>().HasData(new City()
            {
                CityId = Guid.Parse("5458405B-0DA0-4E1C-8DCC-EF07A7C33D84"),
                CityName = "Chandigarh"
            });
        }
    }
}