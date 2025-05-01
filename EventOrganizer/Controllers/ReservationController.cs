using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventOrganizer.Models;
using ClosedXML.Excel;
using System.IO;
using System.Collections.Generic;
using EventOrganizer.Database;

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
            var reservations = await _db.Reservations
                                        .Include(r => r.Event)
                                        .Include(r => r.User)
                                        .ToListAsync();
            return Ok(reservations);
        }

        [HttpGet]
        [Route("GetReservationById")]
        public async Task<IActionResult> GetReservationById(int id)
        {
            var reservation = await _db.Reservations
                                       .Include(r => r.Event)
                                       .Include(r => r.User)
                                       .FirstOrDefaultAsync(r => r.ReservationID == id);

            if (reservation == null)
            {
                return NotFound();
            }

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
                return NotFound($"Client me Id {reservation.UserID} nuk ekziston.");
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
                return NotFound($"Client me Id {reservation.UserID} nuk ekziston.");
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
        [Route("DeleteReservation/{id}")]
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


        [HttpGet]
        [Route("ExportReservationsToExcel")]
        public async Task<IActionResult> ExportUsersToExcel()
        {
            var reservations = await _db.Reservations
                                        .Include(r => r.Event)
                                        .Include(r => r.User)
                                        .ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Reservations");

                // Shtojmë headerat
                worksheet.Cell(1, 1).Value = "Reservation ID";
                worksheet.Cell(1, 2).Value = "Name";
                worksheet.Cell(1, 3).Value = "Surname";
                worksheet.Cell(1, 4).Value = "Reservation Date";
                worksheet.Cell(1, 5).Value = "Total Price";
                worksheet.Cell(1, 6).Value = "Event Name";
                worksheet.Cell(1, 7).Value = "User Email";


                for (int i = 0; i < reservations.Count; i++)
                {
                    var reservation = reservations[i];
                    worksheet.Cell(i + 2, 1).Value = reservation.ReservationID;
                    worksheet.Cell(i + 2, 2).Value = reservation.Name;
                    worksheet.Cell(i + 2, 3).Value = reservation.Surname;
                    worksheet.Cell(i + 2, 4).Value = reservation.ReservationDate.ToString("yyyy-MM-dd");
                    worksheet.Cell(i + 2, 5).Value = reservation.TotalPrice;
                    worksheet.Cell(i + 2, 6).Value = reservation.Event != null ? reservation.Event.EventName : "N/A";
                    worksheet.Cell(i + 2, 7).Value = reservation.User != null ? reservation.User.Email : "N/A";
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Reservations.xlsx");
                }
            }
        }
    }
}