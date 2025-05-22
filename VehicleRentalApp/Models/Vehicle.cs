namespace VehicleRentalApp.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text.Json.Serialization;
    using Microsoft.AspNetCore.Mvc.ModelBinding;

    public class Vehicle
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Brand { get; set; }

        [Required]
        [StringLength(50)]
        public string Model { get; set; }

        [Required]
        [Range(1900, 2100)]
        public int Year { get; set; }

        [Required]
        [StringLength(20)]
        public string LicensePlate { get; set; }

        [Required]
        [Range(0.01, 1000)]
        public decimal PricePerDay { get; set; }

        public bool IsAvailable { get; set; }

        [JsonIgnore] // Exclude from JSON deserialization
        //[BindNever]  // Exclude from model binding and validation
        public string OwnerId { get; set; }

        public string? ImagePath { get; set; }

        // New Property for Average Rating
        [Range(0, 5)]
        public double? AverageRating { get; set; }

        // Navigation property for Ratings
        public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
    }
}
