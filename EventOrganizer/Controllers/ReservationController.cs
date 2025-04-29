using EventOrganizer.Database;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventOrganizer.Database;
using EventOrganizer.Models;

namespace EventOrganizer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public ReservationController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        [Route("GetAllList")]
        public async Task<IActionResult> GetAsync()
        {
            var reservations = await _db.Reservations.ToListAsync();
            return Ok(reservations);
        }

        [HttpGet]
        [Route("GetReservationById")]
        public async Task<IActionResult> GetReservationById(int id)
        {
            var reservation = await _db.Reservations.FindAsync(id);
            return Ok(reservation);
        }

        [HttpPost]
        [Route("AddReservation")]

        public async Task<IActionResult> PostAsync(Reservations reservation)
        {
            var existingClient = await _db.User.FindAsync(reservation.UserID);
            var existingEvent = await _db.Events.FindAsync(reservation.EventID);

            if (existingClient == null)
            {
                return NotFound($"Client me Id {reservation.UserID} nuk egziston");
            }

            if (existingEvent == null)
            {
                return NotFound($"Eventi me ID {reservation.EventID} nuk ekziston.");
            }

            var existingReservation = await _db.Reservations
                .Where(r => r.EventID == reservation.EventID && r.ReservationDate == reservation.ReservationDate)
                .FirstOrDefaultAsync();

            if (existingReservation != null)
            {
                return BadRequest("Data për këtë event është rezervuar tashmë.");
            }

            reservation.User = existingClient;
            reservation.Event = existingEvent;

            _db.Reservations.Add(reservation);
            await _db.SaveChangesAsync();
            return Created($"/GetReservationById/{reservation.ReservationID}", reservation);
        }

        [HttpPut]
        [Route("UpdateReservation/{id}")]
        public async Task<IActionResult> PutAsync(int id, [FromBody] Reservations reservation)
        {
            var existingReservation = await _db.Reservations.FindAsync(id);
            if (existingReservation == null)
            {
                return NotFound();
            }

            var existingClient = await _db.User.FindAsync(reservation.UserID);
            var existingEvent = await _db.Events.FindAsync(reservation.EventID);

            if (existingClient == null)
            {
                return NotFound($"Client me Id {reservation.UserID} nuk ekziston");
            }

            if (existingEvent == null)
            {
                return NotFound($"Eventi me ID {reservation.EventID} nuk ekziston.");
            }

            var conflictingReservation = await _db.Reservations
                .Where(r => r.EventID == reservation.EventID && r.ReservationDate == reservation.ReservationDate && r.ReservationID != id)
                .FirstOrDefaultAsync();

            if (conflictingReservation != null)
            {
                return BadRequest("Data për këtë event është rezervuar tashmë.");
            }

            existingReservation.Name = reservation.Name;
            existingReservation.Surname = reservation.Surname;
            existingReservation.ReservationDate = reservation.ReservationDate;
            existingReservation.TotalPrice = reservation.TotalPrice;
            existingReservation.UserID = reservation.UserID;
            existingReservation.EventID = reservation.EventID;


            await _db.SaveChangesAsync();
            return NoContent();
        }


        [HttpDelete]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var reservationToDelete = await _db.Reservations.FindAsync(id);
            if (reservationToDelete == null)
            {
                return NotFound();
            }

            _db.Reservations.Remove(reservationToDelete);
            await _db.SaveChangesAsync();
            return NoContent();
        }

    }
}