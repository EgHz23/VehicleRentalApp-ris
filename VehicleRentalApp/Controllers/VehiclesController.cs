using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using VehicleRentalApp.Data;
using VehicleRentalApp.Models;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace VehicleRentalApp.Controllers
{
    [Authorize]
    public class VehiclesController : Controller
    {
        private readonly VehicleRentalContext _context;
        private readonly IConfiguration _configuration;

        public VehiclesController(VehicleRentalContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Vehicle vehicle, IFormFile? image)
        {
            vehicle.IsAvailable = true;
            var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(ownerId))
            {
                TempData["Error"] = "Unable to determine the owner of this vehicle. Please log in again.";
                return RedirectToAction("Index", "Home");
            }

            vehicle.OwnerId = ownerId;

            ModelState.Remove(nameof(vehicle.OwnerId));

            if (ModelState.IsValid)
            {
                try
                {
                    if (image != null && image.Length > 0)
                    {
                        string blobConnectionString = _configuration.GetConnectionString("AzureBlobStorage");
                        var blobServiceClient = new BlobServiceClient(blobConnectionString);
                        var blobContainerClient = blobServiceClient.GetBlobContainerClient("vehicle-images");
                        await blobContainerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

                        var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(image.FileName)}";
                        var blobClient = blobContainerClient.GetBlobClient(fileName);

                        using (var stream = image.OpenReadStream())
                        {
                            await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = image.ContentType });
                        }

                        vehicle.ImagePath = blobClient.Uri.ToString();
                    }

                    _context.Vehicles.Add(vehicle);
                    await _context.SaveChangesAsync();

                    TempData["Message"] = "Vehicle added successfully!";
                    return RedirectToAction(nameof(MyVehicles));
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"An error occurred while adding the vehicle: {ex.Message}";
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }

            return View(vehicle);
        }
        // View all vehicles owned by the current user
        public async Task<IActionResult> MyVehicles()
        {
            var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(ownerId))
            {
                return Unauthorized("You must be logged in to view your vehicles.");
            }

            var myVehicles = await _context.Vehicles
                .Where(v => v.OwnerId == ownerId)
                .ToListAsync();

            return View(myVehicles);
        }

        // View all available vehicles
        [AllowAnonymous] // Allow access to everyone
        public async Task<IActionResult> Index(string brand, decimal? minPrice, decimal? maxPrice)
{
    var vehicles = _context.Vehicles.AsQueryable();

    if (!string.IsNullOrEmpty(brand))
    {
        vehicles = vehicles.Where(v => v.Brand.Contains(brand));
    }

    if (minPrice.HasValue)
    {
        vehicles = vehicles.Where(v => v.PricePerDay >= minPrice);
    }

    if (maxPrice.HasValue)
    {
        vehicles = vehicles.Where(v => v.PricePerDay <= maxPrice);
    }

    var result = await vehicles.ToListAsync();
    return View(result);
}


        // View details of a specific vehicle
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vehicle = await _context.Vehicles
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vehicle == null)
            {
                return NotFound();
            }

            return View(vehicle);
        }
        [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Delete(int id)
{
    var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrEmpty(ownerId))
    {
        TempData["Error"] = "Unauthorized action.";
        return RedirectToAction(nameof(MyVehicles));
    }

    var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == id && v.OwnerId == ownerId);

    if (vehicle == null)
    {
        TempData["Error"] = "Vehicle not found or you do not have permission to delete it.";
        return RedirectToAction(nameof(MyVehicles));
    }

    try
    {
        _context.Vehicles.Remove(vehicle);
        await _context.SaveChangesAsync();
        TempData["Message"] = "Vehicle deleted successfully.";
    }
    catch (Exception ex)
    {
        TempData["Error"] = "An error occurred while deleting the vehicle.";
        Console.WriteLine($"Error: {ex.Message}");
    }

    return RedirectToAction(nameof(MyVehicles));
}
[HttpGet]
public async Task<IActionResult> Edit(int id)
{
    var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrEmpty(ownerId))
    {
        return Unauthorized("You must be logged in to edit a vehicle.");
    }

    var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == id && v.OwnerId == ownerId);

    if (vehicle == null)
    {
        TempData["Error"] = "Vehicle not found or you do not have permission to edit it.";
        return RedirectToAction(nameof(MyVehicles));
    }

    return View(vehicle);
}
[HttpPost]
[ValidateAntiForgeryToken]

public async Task<IActionResult> Edit(int id, Vehicle vehicle, IFormFile? image)
{
    var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrEmpty(ownerId))
    {
        return Unauthorized("You must be logged in to edit a vehicle.");
    }

    var existingVehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == id && v.OwnerId == ownerId);

    if (existingVehicle == null)
    {
        TempData["Error"] = "Vehicle not found or you do not have permission to edit it.";
        return RedirectToAction(nameof(MyVehicles));
    }

    // Remove OwnerId from ModelState validation
    ModelState.Remove(nameof(vehicle.OwnerId));

    if (ModelState.IsValid)
    {
        try
        {
            // Update vehicle details
            existingVehicle.Brand = vehicle.Brand;
            existingVehicle.Model = vehicle.Model;
            existingVehicle.Year = vehicle.Year;
            existingVehicle.LicensePlate = vehicle.LicensePlate;
            existingVehicle.PricePerDay = vehicle.PricePerDay;
            existingVehicle.IsAvailable = vehicle.IsAvailable;

            // Update the image if a new one is uploaded
            if (image != null && image.Length > 0)
            {
                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/vehicles");
                Directory.CreateDirectory(uploadsPath);

                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(image.FileName)}";
                var filePath = Path.Combine(uploadsPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                existingVehicle.ImagePath = $"/images/vehicles/{fileName}";
            }

            _context.Vehicles.Update(existingVehicle);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Vehicle updated successfully!";
            return RedirectToAction(nameof(MyVehicles));
        }
        catch (Exception ex)
        {
            TempData["Error"] = "An error occurred while updating the vehicle.";
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
    else
    {
        TempData["Error"] = "Please correct the errors in the form and try again.";

        foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
        {
            Console.WriteLine($"Validation Error: {error.ErrorMessage}");
        }
    }

    return View(vehicle);
}
[HttpGet]
public async Task<IActionResult> RentalForm(int vehicleId)
{
    var vehicle = await _context.Vehicles.FindAsync(vehicleId);
    if (vehicle == null)
    {
        TempData["Error"] = "Vehicle not found.";
        return RedirectToAction(nameof(MyVehicles));
    }

    return View(vehicle);
}

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> RentalForm(int vehicleId, string renterName, string renterDriveLNumber, DateTime startDate, DateTime endDate)
{
    // Basic validation
    if (string.IsNullOrEmpty(renterName) || string.IsNullOrEmpty(renterDriveLNumber) || vehicleId == 0 || startDate >= endDate)
    {
        TempData["Error"] = "Invalid data. Please ensure all fields are filled correctly.";
        return RedirectToAction(nameof(Index));
    }

    var vehicle = await _context.Vehicles.FindAsync(vehicleId);
    if (vehicle == null || !vehicle.IsAvailable)
    {
        TempData["Error"] = "Vehicle not available for rent.";
        return RedirectToAction(nameof(Index));
    }

    var rental = new Rental
    {
        VehicleId = vehicleId,
        RenterName = renterName,
        RenterDriveLNumber = renterDriveLNumber,
        StartDate = startDate,
        EndDate = endDate,
        Status = "Active"
    };

    try
    {
        // Add rental and update vehicle availability
        _context.Rentals.Add(rental);
        vehicle.IsAvailable = false; // Mark the vehicle as unavailable
        await _context.SaveChangesAsync();

        TempData["Message"] = "Rental created successfully!";
        Console.WriteLine($"Rental added: {rental.RenterName}, Vehicle ID: {rental.VehicleId}");
    }
    catch (Exception ex)
    {
        // Log any exception
        TempData["Error"] = "An error occurred while saving the rental.";
        Console.WriteLine($"Error while saving rental: {ex.Message}");
        return RedirectToAction(nameof(Index));
    }

    return RedirectToAction(nameof(MyVehicles));
}

public async Task<IActionResult> MyRents()
{
    var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    Console.WriteLine($"OwnerId: {ownerId}");

    var rentals = await _context.Rentals
        .Include(r => r.Vehicle) // Include vehicle details
        .Where(r => r.Vehicle.OwnerId == ownerId)
        .ToListAsync();

    if (!rentals.Any())
    {
        Console.WriteLine("No rentals found for the logged-in owner.");
    }
    else
    {
        foreach (var rental in rentals)
        {
            Console.WriteLine($"Rental: {rental.RenterName}, Vehicle: {rental.Vehicle?.Brand}, StartDate: {rental.StartDate}");
        }
    }

    return View(rentals);
}
[HttpGet]
public async Task<IActionResult> PrintRental(int id)
{
    var rental = await _context.Rentals
        .Include(r => r.Vehicle) // Include vehicle details
        .FirstOrDefaultAsync(r => r.Id == id);

    if (rental == null)
    {
        TempData["Error"] = "Rental not found.";
        return RedirectToAction(nameof(MyRents));
    }

    return View(rental);
}

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> AddRating(int vehicleId, int stars)
{
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrEmpty(userId))
    {
        TempData["Error"] = "You must be logged in to rate a vehicle.";
        return RedirectToAction(nameof(Index));
    }

    if (stars < 1 || stars > 5)
    {
        TempData["Error"] = "Invalid rating. Please select between 1 and 5 stars.";
        return RedirectToAction(nameof(Details), new { id = vehicleId });
    }

    var rating = new Rating
    {
        VehicleId = vehicleId,
        UserId = userId,
        Stars = stars
    };

    try
    {
        _context.Ratings.Add(rating);
        await _context.SaveChangesAsync();

        // Recalculate the average rating on the client side
        var ratings = await _context.Ratings
            .Where(r => r.VehicleId == vehicleId)
            .ToListAsync();

        var averageRating = ratings.Any() ? ratings.Average(r => r.Stars) : 0;

        var vehicle = await _context.Vehicles.FindAsync(vehicleId);
        if (vehicle != null)
        {
            vehicle.AverageRating = averageRating;
            _context.Vehicles.Update(vehicle);
            await _context.SaveChangesAsync();
        }

        TempData["Message"] = "Thank you for rating this vehicle!";
    }
    catch (Exception ex)
    {
        TempData["Error"] = "An error occurred while saving your rating.";
        Console.WriteLine($"Error: {ex.Message}");
    }

    return RedirectToAction(nameof(Details), new { id = vehicleId });
}
[HttpGet]
public async Task<IActionResult> Details(int id)
{
    var vehicle = await _context.Vehicles
        .FirstOrDefaultAsync(v => v.Id == id);

    if (vehicle == null)
    {
        TempData["Error"] = "Vehicle not found.";
        return RedirectToAction("Index");
    }

    // Fetch all ratings for the vehicle and calculate the average
    var ratings = await _context.Ratings
        .Where(r => r.VehicleId == id)
        .ToListAsync();

    vehicle.AverageRating = ratings.Any() ? ratings.Average(r => r.Stars) : 0;

    return View(vehicle);
}
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> SendMessage(int vehicleId, string email, string phone, DateTime startDate, DateTime endDate, string message)
{
    var senderId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Logged-in user (sender)
    if (string.IsNullOrEmpty(senderId))
    {
        TempData["Error"] = "You must be logged in to send a message.";
        return RedirectToAction(nameof(Index), "Vehicles");
    }

    var vehicle = await _context.Vehicles.FindAsync(vehicleId);
    if (vehicle == null)
    {
        TempData["Error"] = "Vehicle not found.";
        return RedirectToAction(nameof(Index), "Vehicles");
    }

    var messageEntry = new Message
    {
        VehicleId = vehicleId,
        SenderId = senderId,              // Set sender's user ID
        ReceiverId = vehicle.OwnerId,     // Set receiver's user ID (vehicle owner)
        SenderEmail = email,
        SenderPhone = phone,
        StartDate = startDate,
        EndDate = endDate,
        Content = message,
        SentAt = DateTime.UtcNow
    };

    try
    {
        _context.Messages.Add(messageEntry);
        await _context.SaveChangesAsync();
        TempData["Message"] = "Your message has been sent successfully!";
    }
    catch (Exception ex)
    {
        TempData["Error"] = "An error occurred while sending your message.";
        Console.WriteLine($"Error: {ex.Message}");
    }

    return RedirectToAction("Details", "Vehicles", new { id = vehicleId });
}
}   
}