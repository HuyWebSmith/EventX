using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventX.Models
{
    public class EventImage
    {
        [Key]
        public int ImageID { get; set; }

        [Required]
        [ForeignKey("Event")]
        public int EventID { get; set; }


        [StringLength(255)]
        public  string? ImageURL { get; set; }

        public  Event? Event { get; set; }
    }
}
