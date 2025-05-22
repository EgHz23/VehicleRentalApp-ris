using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VehicleRentalApp.Data;
using VehicleRentalApp.Models;

namespace VehicleRentalApp.Controllers
{
    [Authorize(Roles = "Admin")] // Restrict to Admins only
    public class AdminController : Controller
    {
        private readonly VehicleRentalContext _context;

        public AdminController(VehicleRentalContext context)
        {
            _context = context;
        }

        // GET: Admin/AvailableVehicles
        public async Task<IActionResult> AvailableVehicles()
        {
            var vehicles = await _context.Vehicles.ToListAsync();
            return View(vehicles);
        }

        // POST: Admin/DeleteVehicle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteVehicle(int id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null)
            {
                TempData["Error"] = "Vehicle not found.";
                return RedirectToAction(nameof(AvailableVehicles));
            }

            _context.Vehicles.Remove(vehicle);
            await _context.SaveChangesAsync();
            TempData["Message"] = "Vehicle deleted successfully.";
            return RedirectToAction(nameof(AvailableVehicles));
        }
    }
}
