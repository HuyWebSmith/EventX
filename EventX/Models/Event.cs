using EventX.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static EventX.Models.Category;

namespace EventX.Models
{
    public class Event
    {
        [Key]
        public int EventID { get; set; }
        [Required]
        public string OrganizerId { get; set; }   // Khóa ngoại (string)

        [Required]
        [EmailAddress]
        public required string OrganizerEmail { get; set; }
        [Required]
        [StringLength(255)]
        public string? OrganizerName { get; set; }  // Tên ban tổ chức

        [StringLength(1000)]
        public string? OrganizerInfo { get; set; }  // Thông tin về ban tổ chức

        public string? OrganizerLogoUrl { get; set; }  // URL của logo ban tổ chức
        public string? OrganizerBannerUrl { get; set; }  // URL của banner ban tổ chức
        [Required]
        [StringLength(255)]
        public required string Title { get; set; }
        // Mô tả sự kiện
        public string? Description { get; set; }  // Mô tả chi tiết về sự kiện

       
        //Ngay dien ra su kien
        public DateTime EventDate { get; set; }
        [Required]
        public EventStatus Status { get; set; } = EventStatus.Pending;
        // Thời gian bắt đầu và kết thúc sự kiện
        [Required]
        public DateTime EventStartTime { get; set; }  // Thời gian bắt đầu

        [Required]
        public DateTime EventEndTime { get; set; }  // Thời gian kết thúc

        [MaxLength(1000)]
        public string? BuyerMessage { get; set; }  // Lời nhắn từ nhà tổ chức đến người mua vé
        //Ngay hien tai
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int CategoryId { get; set; }
        public Category? Category { get; set; } = null!;

        public required ICollection<EventImage> EventImages { get; set; } = new List<EventImage>();
        public  ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
        // Liên kết với Payment
        public ICollection<PaymentInfo> PaymentInfos { get; set; } = new List<PaymentInfo>();

        // Liên kết với RedInvoice
        public ICollection<RedInvoice> RedInvoices { get; set; } = new List<RedInvoice>();
        public ICollection<Location> Locations { get; set; } = new List<Location>();

        public ICollection<Review> Reviews { get; set; } = new List<Review>();

        //public Dictionary<TicketType, decimal> TicketPrices { get; set; } = new Dictionary<TicketType, decimal>();

    }
}
