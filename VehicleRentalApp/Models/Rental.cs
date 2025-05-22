using System;
using System.ComponentModel.DataAnnotations;

namespace VehicleRentalApp.Models
{
    public class Rental
    {
        public int Id { get; set; }

        [Required]
        public int VehicleId { get; set; }
        public Vehicle Vehicle { get; set; }

        [Required]
        [StringLength(100)]
        public string RenterName { get; set; }

        [Required]
        [StringLength(100)]
        public string RenterDriveLNumber { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Active";
    }
}
