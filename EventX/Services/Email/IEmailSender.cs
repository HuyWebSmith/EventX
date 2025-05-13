namespace EventX.Services.Email
{
    public interface IEmailSender
    {
        Task SendEmailWithQrAsync(
                string toEmail,
         string subject,
         string ticketCode,
         byte[] qrImage,
         string eventName,
         int eventId,
         string eventDate,
         decimal ticketPrice,
         int quantity,
         decimal totalAmount,
         string eventLocation,
         string ticketName);
        
    }
}
