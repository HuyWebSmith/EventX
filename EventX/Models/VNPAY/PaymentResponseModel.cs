namespace EventX.Models.VNPAY
{
    public class PaymentResponseModel
    {
        public string OrderDescription { get; set; }
        public string TransactionId { get; set; }
        public string OrderId { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentId { get; set; }
        public bool Success { get; set; }
        public string Token { get; set; }
        public string VnPayResponseCode { get; set; }
        public bool IsPaymentSuccessful()
        {
            return VnPayResponseCode == "00"; // Mã phản hồi thành công từ VNPAY
        }

    }
}
