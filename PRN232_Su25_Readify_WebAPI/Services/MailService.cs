using MailKit.Security;
using MimeKit;
using MailKit.Net.Smtp;
using PRN232_Su25_Readify_WebAPI.Dtos;
using PRN232_Su25_Readify_WebAPI.Services.IServices;

namespace PRN232_Su25_Readify_WebAPI.Services
{
    public class MailService : IMail
    {
        private readonly EmailConfiguration _emailConfig;
        public MailService(EmailConfiguration emailConfig)
        {
            _emailConfig = emailConfig;
        }
        private MimeMessage CreateEmailMessage(Message message)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("email", _emailConfig.From));
            emailMessage.To.AddRange(message.To);
            emailMessage.Subject = message.Subject;

            var builder = new BodyBuilder
            {
                HtmlBody = message.Content
            };

            emailMessage.Body = builder.ToMessageBody();
            return emailMessage;
        }

        private async Task SendAsync(MimeMessage mailMessage)
        {
            using var client = new SmtpClient();
            try
            {
                await client.ConnectAsync(_emailConfig.SmtpServer, _emailConfig.Port, SecureSocketOptions.StartTls, CancellationToken.None);
                await client.AuthenticateAsync(_emailConfig.UserName, _emailConfig.Password, CancellationToken.None);
                await client.SendAsync(mailMessage, CancellationToken.None);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
                throw;
            }
            finally
            {
                client.Disconnect(true);
                client.Dispose();
            }
        }
        public async Task SendEmail(Message message)
        {
            var emailMessage = CreateEmailMessage(message);
            await SendAsync(emailMessage);
        }

        public async Task SendMailConfirmAsync(string email, string urlUser)
        {
            var subject = "Xác thực tài khoản";
            var content = $@"<!DOCTYPE html>
<html>

<head>
    <meta charset=""UTF-8"">
    <title>Xác Thực Tài Khoản</title>
    <style>
        body {{
            font-family: Arial, sans-serif;
            background-color: #f4f4f4;
            margin: 0;
            padding: 20px;
        }}

        .container {{
            max-width: 600px;
            background: #ffffff;
            padding: 20px;
            border-radius: 8px;
            box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
            text-align: center;
        }}

        .btn {{
            display: inline-block;
            background-color: #007bff;
            color: #ffffff;
            padding: 12px 20px;
            text-decoration: none;
            font-size: 16px;
            border-radius: 5px;
            margin-top: 20px;
        }}

        .btn:hover {{
            background-color: #0056b3;
        }}

        .footer {{
            margin-top: 20px;
            font-size: 12px;
            color: #777;
        }}
    </style>
</head>

<body>
    <div class=""container"">
        <h2>Chào mừng bạn đến với Readify</h2>
        <p>Cảm ơn bạn đã đăng ký. Vui lòng xác thực email của bạn bằng cách nhấp vào nút bên dưới:</p>
        <a href=""{urlUser}"" class=""btn"">Xác Thực Tài Khoản</a>
        <p>Nếu bạn không đăng ký tài khoản, vui lòng bỏ qua email này.</p>
        <div class=""footer"">
            <p>&copy; 2025 PRN222_G4. Mọi quyền được bảo lưu.</p>
        </div>
    </div>
</body>

</html>";
            var message = new Message(new string[] { email }, subject, content);
            await SendEmail(message);
        }

        public async Task SendMailForgotPassWord(string email, string token)
        {
            var subject = "Khôi phục mật khẩu";
            var content = $@"<!DOCTYPE html>
<html lang=""vi"">

<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Đặt lại mật khẩu</title>
    <style>
        body {{
            font-family: Arial, sans-serif;
            background-color: #f4f4f4;
            padding: 20px;
            text-align: center;
        }}

        .container {{
            max-width: 500px;
            background: #ffffff;
            padding: 20px;
            border-radius: 8px;
            box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
            margin: auto;
        }}

        h2 {{
            color: #333;
        }}

        p {{
            color: #555;
        }}

        .btn {{
            display: inline-block;
            padding: 10px 20px;
            background: #007bff;
            color: #ffffff;
            text-decoration: none;
            border-radius: 5px;
            font-weight: bold;
        }}

        .btn:hover {{
            background: #0056b3;
        }}
    </style>
</head>

<body>
    <div class=""container"">
        <h2>Yêu cầu đặt lại mật khẩu</h2>
        <p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn.</p>
        <p>Nhấn vào nút bên dưới để tiếp tục:</p>
        <a href=""{token}"" class=""btn"">Đặt lại mật khẩu</a>
        <p>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.</p>
        <p>Trân trọng,<br>Đội ngũ hỗ trợ</p>
    </div>
</body>

</html>";
            var message = new Message(new string[] { email }, subject, content);
            await SendEmail(message);
        }
    }
}
