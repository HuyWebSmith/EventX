using EventX.Enums;
using System.ComponentModel.DataAnnotations;

namespace EventX.ViewModels
{
    public class EventCreateViewModel
    {
        [Required]
        public string OrganizerEmail { get; set; } = string.Empty;

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public DateTime EventDate { get; set; }

        public int LocationId { get; set; }
        public int CategoryId { get; set; }

        public List<TicketInputModel> Tickets { get; set; } = Enum
            .GetValues<TicketType>()
            .Select(t => new TicketInputModel { Type = t })
            .ToList();
    }

    public class TicketInputModel
    {
        public TicketType Type { get; set; }

        [Required]
        [Range(0, 999999)]
        public decimal Price { get; set; }

        [Required]
        [Range(1, 100000)]
        public int Quantity { get; set; }
    }
}
