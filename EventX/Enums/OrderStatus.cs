namespace EventX.Enums
{
    public enum OrderStatus
    {
        Pending,     // Đang chờ xử lý
        Confirmed,   // Đã xác nhận
        Paid,        // Đã thanh toán
        Cancelled,   // Đã hủy
        Refunded     // Đã hoàn tiền
    }
}
