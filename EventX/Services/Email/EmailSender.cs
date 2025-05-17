using EventX.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace EventX.Services.Email
{
    // Dịch vụ EmailSender
    public class EmailSender : IEmailSender
    {
        private readonly EmailSettings _emailSettings;

        public EmailSender(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
            Console.WriteLine($"SMTP Server: {_emailSettings.SmtpServer}");
            Console.WriteLine($"From Email: {_emailSettings.FromEmail}");

        }

        public async Task SendEmailWithQrAsync(
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
     string ticketName,
     string organizerMessage)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;

            var builder = new BodyBuilder();
            builder.HtmlBody = $@"
<html>
<head>
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background-color: #f5f5f5;
            color: #333;
            padding: 30px;
        }}
        .container {{
            background-color: #ffffff;
            border-radius: 12px;
            padding: 30px;
            max-width: 700px;
            margin: auto;
            box-shadow: 0 8px 20px rgba(0, 0, 0, 0.1);
        }}
        .header {{
            background-color: #007bff;
            color: white;
            padding: 20px;
            border-radius: 12px 12px 0 0;
            text-align: center;
        }}
        .qr-code {{
            text-align: center;
            margin: 30px 0;
        }}
        .order-table {{
            width: 100%;
            border-collapse: collapse;
            margin-top: 20px;
        }}
        .order-table th, .order-table td {{
            padding: 12px;
            border-bottom: 1px solid #e0e0e0;
            text-align: left;
        }}
        .order-table th {{
            background-color: #f1f1f1;
        }}
        .button {{
            display: inline-block;
            background-color: #28a745;
            color: white;
            padding: 12px 24px;
            text-decoration: none;
            border-radius: 6px;
            margin-top: 30px;
        }}
        .footer {{
            margin-top: 40px;
            font-size: 13px;
            text-align: center;
            color: #888;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>🎉 Đặt vé thành công!</h2>
        </div>
        <p>Xin chào,</p>
        <p>Cảm ơn bạn đã đặt vé tham dự sự kiện. Dưới đây là thông tin chi tiết:</p>

        <p><strong>Mã vé:</strong> {ticketCode}</p>

        <div class='qr-code'>
            <p><strong>Mã QR để check-in:</strong></p>
            <img src='cid:qrcode' alt='QR Code' style='width:160px; height:160px;' />
        </div>

        <h3>📝 Chi tiết đơn hàng</h3>
        <table class='order-table'>
            <tr>
                <th>Tên sự kiện</th>
                <td>{eventName}</td>
            </tr>
            <tr>
                <th>Địa điểm</th>
                <td>{eventLocation}</td>
            </tr>
            <tr>
                <th>Tên vé</th>
                <td>{ticketName}</td>
            </tr>
            <tr>
                <th>Ngày & Giờ</th>
                <td>{eventDate}</td>
            </tr>
            <tr>
                <th>Giá vé</th>
                <td>{ticketPrice:C}</td>
            </tr>
            <tr>
                <th>Số lượng</th>
                <td>{quantity}</td>
            </tr>
            <tr>
                <th>Tổng cộng</th>
                <td>{totalAmount:C}</td>
            </tr>
        </table>

        <div style='text-align: center;'>
            <a href='https://localhost:7236/Home/Display?eventId={eventId}' class='button'>Xem chi tiết sự kiện</a>
        </div>
        <h3>📣 Tin nhắn từ ban tổ chức</h3>
        <div style='background-color: #f8f9fa; border-left: 4px solid #007bff; padding: 15px; margin: 20px 0;'>
            {organizerMessage}
        </div>

        <div class='footer'>
            <p>Trân trọng,</p>
            <p><strong>Đội ngũ EventX</strong></p>
        </div>
    </div>
</body>
</html>";

            // Gắn QR code vào email
            builder.LinkedResources.Add("qr.png", qrImage).ContentId = "qrcode";

            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

    }
}
