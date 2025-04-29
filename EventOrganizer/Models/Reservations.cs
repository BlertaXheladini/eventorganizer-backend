using Microsoft.OpenApi.Models;
using System.ComponentModel.DataAnnotations;

namespace EventOrganizer.Models
{
    public class Reservations
    {
            [Key]
            public int ReservationID { get; set; }

            public string Name { get; set; }
            public string Surname { get; set; }
        public DateOnly ReservationDate { get; set; }
        public double TotalPrice { get; set; }
            public int UserID { get; set; }
            public int EventID { get; set; }

            public User User { get; set; }

            public Events Event { get; set; }

        }
    }
