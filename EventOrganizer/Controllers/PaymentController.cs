using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EventOrganizer.Models;
using EventOrganizer.Database;

namespace OrganizingEvents.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public PaymentController(ApplicationDbContext db)
        {
            _db = db;
        }

        //ListAll
        [HttpGet]
        [Route("GetAllList")]
        public async Task<IActionResult> GetAsync()
        {
            var payment = await _db.Payment.Include(q => q.Reservations).ToListAsync();
            return Ok(payment);
        }

        //GetById
        [HttpGet]
        [Route("GetPaymentById")]
        public async Task<IActionResult> GetPagesaByIdAsync(int id)
        {
            var payment = await _db.Payment.Include(q => q.Reservations).FirstOrDefaultAsync(q => q.Id == id);
            return Ok(payment);
        }

        //Add
        [HttpPost]
        [Route("AddPayment")]
        public async Task<IActionResult> PostAsync(Payment payment)
        {
            var existingReservation = await _db.Reservations.FindAsync(payment.ReservationId);
            if (existingReservation == null)
            {
                return NotFound($"Reservation me ID {payment.ReservationId} nuk ekziston.");
            }

            payment.Reservations = existingReservation;
            _db.Payment.Add(payment);
            await _db.SaveChangesAsync();
            return Created($"id/{payment.Id}", payment);
        }


        //Update
        [HttpPut]
        [Route("Update/{id}")]
        public async Task<IActionResult> PutAsync(Payment payment)
        {
            var existingReservation = await _db.Reservations.FindAsync(payment.ReservationId);
            if (existingReservation == null)
            {
                return NotFound($"Reservation me ID {payment.ReservationId} nuk ekziston.");
            }

            payment.Reservations = existingReservation;
            _db.Payment.Update(payment);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        //Delete
        [HttpDelete]
        [Route("Delete")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var paymentIdDelete = await _db.Payment.Include(q => q.Reservations).FirstOrDefaultAsync(q => q.Id == id);

            if (paymentIdDelete == null)
            {
                return NotFound();
            }

            _db.Payment.Remove(paymentIdDelete);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}