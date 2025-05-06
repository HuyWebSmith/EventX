using System.ComponentModel.DataAnnotations;

namespace EventX.Models
{
    public class Category
    {
            [Key]
            public int CategoryId { get; set; }

            [Required]
            [StringLength(100)]
            public string Name { get; set; } = null!;

            [StringLength(255)]
            public string? Description { get; set; }

            public bool IsActive { get; set; } = true;

            public DateTime CreatedAt { get; set; } = DateTime.Now;

            public ICollection<Event> Events { get; set; } = new List<Event>();

    }
}
