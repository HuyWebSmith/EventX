using EventX.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventX.Models
{
    public class Order
    {
        [Key]
        public int OrderID { get; set; }
        public required string UserID { get; set; } // Identity sử dụng string thay vì int

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        public OrderStatus OrderStatus { get; set; } = OrderStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public required string Notes { get; set; }

        // Quan hệ với IdentityUser
        [ForeignKey("UserID")]
        [ValidateNever]
        public required ApplicationUser ApplicationUser { get; set; }
        public required List<OrderDetail> OrderDetails { get; set; }
    }
}
