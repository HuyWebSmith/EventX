using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventX.Models
{
    public class PaymentInfo
    {
        [Key]
        public int PaymentInfoID { get; set; }

        [Required]
        [MaxLength(100)]
        public string AccountHolder { get; set; }  // Chủ tài khoản

        [Required]
        [MaxLength(20)]
        public string AccountNumber { get; set; }  // Số tài khoản

        [Required]
        [MaxLength(100)]
        public string BankName { get; set; }  // Tên ngân hàng

        [MaxLength(100)]
        public string? Branch { get; set; }  // Chi nhánh

        // Liên kết với người tạo (User hoặc Event Organizer)
        public string? CreatorId { get; set; }  // ID người tạo (liên kết với AspNetUsers nếu dùng Identity)

        public int? EventId { get; set; }

        [ForeignKey("EventId")]
        public Event? Event { get; set; }
    }
}
