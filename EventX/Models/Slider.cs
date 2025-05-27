using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventX.Models
{
    public class Slider
    {
        public int Id { get; set; }

        public string? Title { get; set; }

        public string? Description { get; set; }

        public string? ImageUrl { get; set; }

        public string? Link { get; set; }

        public int Order { get; set; } = 0; // Thứ tự hiển thị

        public bool IsActive { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        [NotMapped]
        public IFormFile ImageFile { get; set; }
        public string? VideoUrl { get; set; }
        [NotMapped]
        public IFormFile VideoFile { get; set; }

    }
}
