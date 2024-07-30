using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TechnoEvents.Models
{
    public class Event
    {
        public int EventId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        [StringLength(1000)]
        public string Description { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public bool IsPublic { get; set; }

        public string? Location { get; set; }

        public int UserId { get; set; }

        public User? User { get; set; }



    }
}
