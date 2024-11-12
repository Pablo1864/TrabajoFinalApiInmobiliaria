using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

public class EmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var smtpConfig = _configuration.GetSection("Smtp");
        var smtpClient = new SmtpClient(smtpConfig["Server"])
        {
            Port = int.Parse(smtpConfig["Port"]),
            Credentials = new NetworkCredential(smtpConfig["SenderEmail"], smtpConfig["Password"]),
            EnableSsl = bool.Parse(smtpConfig["EnableSsl"])
        };

        // Cambia el cuerpo del mensaje a texto plano
        var mailMessage = new MailMessage
        {
            From = new MailAddress(smtpConfig["SenderEmail"]),
            Subject = subject,
            Body = body,
            IsBodyHtml = true // Enviar en formato de texto plano
        };

        mailMessage.To.Add(toEmail);

        await smtpClient.SendMailAsync(mailMessage);
    }
}
