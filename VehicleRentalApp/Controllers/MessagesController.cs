using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehicleRentalApp.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims; // For ClaimTypes



namespace VehicleRentalApp.Controllers
{
    [Authorize]
    public class MessagesController : Controller
    {
        private readonly VehicleRentalContext _context;

        public MessagesController(VehicleRentalContext context)
        {
            _context = context;
        }

        [HttpGet]
public async Task<IActionResult> ReceivedMessages()
{
    var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    Console.WriteLine($"OwnerId: {ownerId}");

    if (string.IsNullOrEmpty(ownerId))
    {
        TempData["Error"] = "You must be logged in to view messages.";
        return RedirectToAction("Index", "Vehicles");
    }

    try
    {
        var messages = await _context.Messages
            .Include(m => m.Vehicle)
            .Where(m => m.ReceiverId == ownerId)
            .OrderByDescending(m => m.SentAt)
            .ToListAsync();

        Console.WriteLine($"Messages found: {messages.Count}");
        return View(messages);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error fetching messages: {ex.Message}");
        TempData["Error"] = "An error occurred while fetching messages.";
        return RedirectToAction("Index", "Vehicles");
    }
}
    }
}

