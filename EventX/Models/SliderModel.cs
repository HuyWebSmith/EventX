using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventX.Models
{
    public class SliderModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Yêu cầu không bỏ trống tên slider")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Yêu cầu không bỏ trống mô tả")]
        public string Description { get; set; }

        public bool IsActive { get; set; }

        public string ImagePath { get; set; } // Lưu đường dẫn ảnh

        [NotMapped]
        public IFormFile ImageUpLoad { get; set; } // Ảnh upload

        public string Link { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

    }
}
