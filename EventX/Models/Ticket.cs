using EventX.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventX.Models
{
    public class Ticket
    {
        [Key]
        public int TicketID { get; set; }

        [ForeignKey("Event")]
        public int EventID { get; set; }

        [Required]
        public TicketType Type { get; set; } = TicketType.GeneralAdmission; // Mặc định là vé thường

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [Required]
        public int Quantity { get; set; }

        public int Sold { get; set; } = 0;

        [Required]
        public DateTime StartDate { get; set; }  // Thời gian bắt đầu

        [Required]
        public DateTime EndDate { get; set; }  // Thời gian kết thúc

        // Quan hệ với Event
        public  Event? Event { get; set; }

        // Thông tin thêm
        [MaxLength(500)]
        public string Description { get; set; }  // Mô tả loại vé

        [MaxLength(100)]
        public string? TicketCode { get; set; }  // Mã vé duy nhất

        [Column(TypeName = "decimal(5,2)")]
        public decimal? Discount { get; set; }  // Giảm giá cho vé, nullable

        public string Currency { get; set; }  // Tiền tệ của giá vé (ví dụ: "VND", "USD")
        
    }
}
