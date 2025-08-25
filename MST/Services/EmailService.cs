using System.Net;
using System.Net.Mail;

namespace MST.Services
{
    public class EmailService: IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task SendEmailAsync(string toEmail,string subject,string body)
        {
            //throw new NotImplementedException();
            var from = _configuration["EmailSetting:From"];
            var smtpServer = _configuration["EmailSetting:SmtpServer"];
            var port = int.Parse(_configuration["EmailSetting:Port"]!);
            var username = _configuration["EmailSetting:Username"];
            var password = _configuration["EmailSetting:Password"];

            var message = new MailMessage(from!, toEmail, subject, body);
            message.IsBodyHtml = true;

            using var client = new SmtpClient(smtpServer, port)
            {
                Credentials = new NetworkCredential(username, password),
                EnableSsl = true
            };
            await client.SendMailAsync(message);
        }

    }
}
