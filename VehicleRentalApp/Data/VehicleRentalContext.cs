using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VehicleRentalApp.Models;

namespace VehicleRentalApp.Data
{
    public class VehicleRentalContext : IdentityDbContext<ApplicationUser>
    {
        public VehicleRentalContext(DbContextOptions<VehicleRentalContext> options)
            : base(options)
        {
        }

        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Rental> Rentals { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
{
    base.OnModelCreating(builder);

    // Configure Vehicle -> ApplicationUser (Owner) relationship
    builder.Entity<Vehicle>()
        .HasOne<ApplicationUser>()
        .WithMany()
        .HasForeignKey(v => v.OwnerId)
        .OnDelete(DeleteBehavior.Restrict); // Use Restrict instead of Cascade

    // Configure Rental -> Vehicle relationship
    builder.Entity<Rental>()
        .HasOne(r => r.Vehicle)
        .WithMany()
        .HasForeignKey(r => r.VehicleId)
        .OnDelete(DeleteBehavior.Cascade); // Allow cascading deletes for Rentals

    builder.Entity<Rating>()
        .HasOne(r => r.Vehicle)
        .WithMany()
        .HasForeignKey(r => r.VehicleId)
        .OnDelete(DeleteBehavior.Cascade);

    builder.Entity<Rating>()
        .HasOne(r => r.User)
        .WithMany()
        .HasForeignKey(r => r.UserId)
        .OnDelete(DeleteBehavior.Restrict);
}

    }
}
