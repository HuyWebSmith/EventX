using EventX.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventX.Models
{
    public class RedInvoice
    {
        [Key]
        public int RedInvoiceId { get; set; }

        [Required]

        public BusinessType BusinessType { get; set; }  // Loại hình kinh doanh

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; }  // Họ tên người nhận

        [Required]
        [MaxLength(200)]
        public string Address { get; set; }  // Địa chỉ

        [Required]
        [MaxLength(20)]
        public string TaxCode { get; set; }  // Mã số thuế

        // Liên kết với Event
        [Required]
        public int EventId { get; set; }

        [ForeignKey("EventId")]
        public Event? Event { get; set; }
    }
}
