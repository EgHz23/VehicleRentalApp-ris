using System.ComponentModel.DataAnnotations;

namespace VehicleRentalApp.Models
{
    public class Rating
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public Vehicle Vehicle { get; set; }

        public string UserId { get; set; } // User who rated
        public ApplicationUser User { get; set; }

        [Range(1, 5)]
        public int Stars { get; set; }
    
    }
}

