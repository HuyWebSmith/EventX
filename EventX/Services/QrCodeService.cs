using QRCoder;

namespace EventX.Services
{
    public class QrCodeService
    {
        public byte[] GenerateQrImage(string ticketCode)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrData = qrGenerator.CreateQrCode(ticketCode, QRCodeGenerator.ECCLevel.Q);
            var pngQrCode = new PngByteQRCode(qrData);
            return pngQrCode.GetGraphic(20); // Trả về mảng byte[] PNG
        }
    }
}
