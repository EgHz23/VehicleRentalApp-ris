
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text.Json.Serialization;
    using Microsoft.AspNetCore.Mvc.ModelBinding;


namespace VehicleRentalApp.Models
{
    public class VehicleUploadDto
    {
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

        public IFormFile? Image { get; set; }
    }
}
