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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Member>()
                .HasIndex(b => b.PersonalIdentificationNumber).IsUnique();

            // Uncomment if require FullName to be unique
            //modelBuilder.Entity<Member>()
            //    .HasIndex(b => b.FullName).IsUnique();

            modelBuilder.Entity<Member>()
                .Property(b => b.PersonalIdentificationNumber).HasMaxLength(11);

            modelBuilder.Entity<Member>()
                .Property(b => b.FirstName).HasMaxLength(50);

            modelBuilder.Entity<Member>()
                .Property(b => b.LastName).HasMaxLength(50);

            modelBuilder.Entity<VehicleType>()
                .HasIndex(b => b.Name).IsUnique();

            modelBuilder.Entity<VehicleType>()
                .Property(b => b.Name).HasMaxLength(50);

            modelBuilder.Entity<Vehicle>(entity =>
            {
                entity.HasOne(d => d.VehicleType)
                    .WithMany(p => p.Vehicles)
                    .HasForeignKey(d => d.VehicleTypeId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}

