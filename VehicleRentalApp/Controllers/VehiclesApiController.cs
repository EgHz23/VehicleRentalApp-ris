using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using VehicleRentalApp.Data;
using VehicleRentalApp.Filters;
using VehicleRentalApp.Models;
using Microsoft.AspNetCore.Authorization;


namespace VehicleRentalApp.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class VehiclesApiController : ControllerBase
    {
        private readonly VehicleRentalContext _context;
        private readonly IConfiguration _configuration;

        public VehiclesApiController(VehicleRentalContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

 [HttpPost]
[Consumes("multipart/form-data")]
[SwaggerOperation(
    Summary = "Add a new vehicle with optional image.",
    Description = "Uploads a new vehicle with optional image."
)]
[SwaggerResponse(201, "Vehicle created successfully.")]
[SwaggerResponse(400, "Invalid input data.")]
[ApiKeyAuth]
public async Task<IActionResult> AddVehicle([FromForm] VehicleUploadDto vehicleUpload)
{
    if (!ModelState.IsValid)
    {
        return BadRequest(new { Message = "Invalid data provided.", Errors = ModelState });
    }

    var vehicle = new Vehicle
    {
        Brand = vehicleUpload.Brand,
        Model = vehicleUpload.Model,
        Year = vehicleUpload.Year,
        LicensePlate = vehicleUpload.LicensePlate,
        PricePerDay = vehicleUpload.PricePerDay,
        IsAvailable = true,
        OwnerId = "cfcc2bdd-c780-46a5-852d-9bf58cc858bd" 
    };

    // Handle the image upload
    if (vehicleUpload.Image != null && vehicleUpload.Image.Length > 0)
    {
        var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/vehicles");
        Directory.CreateDirectory(uploadsPath);

        var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(vehicleUpload.Image.FileName)}";
        var filePath = Path.Combine(uploadsPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await vehicleUpload.Image.CopyToAsync(stream);
        }

        vehicle.ImagePath = $"/images/vehicles/{fileName}";
    }

    _context.Vehicles.Add(vehicle);
    await _context.SaveChangesAsync();

    return CreatedAtAction(nameof(AddVehicle), new { id = vehicle.Id }, vehicle);
}





   // Get all available vehicles
        [AllowAnonymous]
        [HttpGet("available")]
        [SwaggerOperation(
            Summary = "Retrieve all available vehicles.",
            Description = "Returns a list of all available vehicles in the database.",
            OperationId = "GetAvailableVehicles",
            Tags = new[] { "Vehicles" })]
        [SwaggerResponse(StatusCodes.Status200OK, "List of available vehicles.", typeof(List<Vehicle>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "No available vehicles found.")]
        public async Task<IActionResult> GetAvailableVehicles()
        {
            var availableVehicles = await _context.Vehicles
                .Where(v => v.IsAvailable)
                .ToListAsync();

            if (!availableVehicles.Any())
            {
                return NotFound("No available vehicles found.");
            }

            return Ok(availableVehicles);
        }

        // Get details of a specific vehicle
        [AllowAnonymous]
        [HttpGet("{id}")]
        [SwaggerOperation(
            Summary = "Get details of a specific vehicle.",
            Description = "Retrieves details of a vehicle by its ID.",
            OperationId = "GetVehicleDetails",
            Tags = new[] { "Vehicles" })]
        [SwaggerResponse(StatusCodes.Status200OK, "Vehicle details retrieved successfully.", typeof(Vehicle))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Vehicle not found.")]
        public async Task<IActionResult> GetVehicleDetails(int id)
        {
            var vehicle = await _context.Vehicles
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vehicle == null)
            {
                return NotFound("Vehicle not found.");
            }

            return Ok(vehicle);
        }
    }
}