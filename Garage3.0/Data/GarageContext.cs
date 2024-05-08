﻿using Garage3.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Garage3.Data
{
    public class GarageContext : DbContext
    {
        public GarageContext(DbContextOptions<GarageContext> options)
        : base(options) { }

        public DbSet<Member> Members => Set<Member>();
        public DbSet<Vehicle> Vehicles => Set<Vehicle>();
        public DbSet<VehicleType> VehicleTypes => Set<VehicleType>();
        public DbSet<SpotOccupation> SpotOccupations => Set<SpotOccupation>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Member>()
                .HasIndex(b => b.PersonalIdentificationNumber).IsUnique();

            // Uncomment if require FullName to be unique
            //modelBuilder.Entity<Member>()
            //    .HasIndex(b => b.FullName).IsUnique();

            modelBuilder.Entity<VehicleType>()
                .HasIndex(b => b.Name).IsUnique();
        }
    }
}

