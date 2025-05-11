using System.ComponentModel.DataAnnotations;

namespace EventX.Enums
{
    public enum OrderStatus
    {
        [Display(Name = "Đang chờ xử lý")]
        Pending,     // Đang chờ xử lý

        [Display(Name = "Đã xác nhận")]
        Confirmed,   // Đã xác nhận

        [Display(Name = "Đã thanh toán")]
        Paid,        // Đã thanh toán

        [Display(Name = "Đã hủy")]
        Cancelled,   // Đã hủy

        [Display(Name = "Đã hoàn tiền")]
        Refunded     // Đã hoàn tiền
    }
}
