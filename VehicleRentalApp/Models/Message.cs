using System;

namespace VehicleRentalApp.Models
{
    public class Message
{
    public int Id { get; set; }

    // Reference to the vehicle the message is about
    public int VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } // Navigation property

    // Sender's user ID
    public string SenderId { get; set; } // Foreign key for the sender (User)

    // Receiver's user ID (Owner of the vehicle)
    public string ReceiverId { get; set; } // Foreign key for the receiver (Owner)

    // Sender's email or contact details
    public string SenderEmail { get; set; }

    public string? SenderPhone { get; set; } // Optional phone number

    public DateTime StartDate { get; set; } // Requested start date
    public DateTime EndDate { get; set; }   // Requested end date

    public string Content { get; set; } // Message content

    public DateTime SentAt { get; set; } // Timestamp of when the message was sent
}

}
