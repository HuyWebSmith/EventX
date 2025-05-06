using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventX.Models
{
    public class Location
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public required string Name { get; set; }  // Tên địa điểm (ví dụ: Tòa nhà ABC, Hội trường XYZ)

        [Required]
        [StringLength(255)]
        public required string FullAddress { get; set; }  // Địa chỉ đầy đủ (Số nhà + Đường phố)

        [StringLength(255)]
        public required string Ward { get; set; }  // Phường xã

        [StringLength(255)]
        public required string District { get; set; }  // Quận huyện

        [StringLength(255)]
        public required string City { get; set; }  // Thành phố

        public int? EventId { get; set; }

        [ForeignKey("EventId")]
        public Event? Event { get; set; }
    }
}
