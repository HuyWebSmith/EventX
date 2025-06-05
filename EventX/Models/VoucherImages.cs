    using System.ComponentModel.DataAnnotations.Schema;

    namespace EventX.Models
    {
        public class VoucherImages
        {
            public int Id { get; set; }
            public string? ImageUrl { get; set; }
            public string? Link { get; set; }
            public int Order { get; set; } = 0; // Thứ tự hiển thị
            public bool IsActive { get; set; }

            public DateTime CreatedDate { get; set; } = DateTime.Now;
            [NotMapped]
            public IFormFile ImageFile { get; set; }

        }
    }
