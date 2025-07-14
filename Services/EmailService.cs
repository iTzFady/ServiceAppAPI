using System.Net;
using System.Net.Mail;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    public EmailService(IConfiguration config)
    {
        _config = config;
    }
    public async Task SendEmailAsync(string email, string subject, string message)
    {

        var host = Environment.GetEnvironmentVariable("EMAIL_HOST");
        var port = int.Parse(Environment.GetEnvironmentVariable("EMAIL_PORT"));
        var username = Environment.GetEnvironmentVariable("EMAIL_USERNAME");
        var password = Environment.GetEnvironmentVariable("EMAIL_PASSWORD");
        var fromEmail = Environment.GetEnvironmentVariable("EMAIL_FROM");
        var fromName = Environment.GetEnvironmentVariable("EMAIL_FROM_NAME");
        using var Client = new SmtpClient(host, port)
        {
            Credentials = new NetworkCredential(username, password),
            EnableSsl = true
        };
        var mailMessage = new MailMessage
        {
            From = new MailAddress(fromEmail, fromName),
            Subject = subject,
            Body = message,
            IsBodyHtml = true
        };
        mailMessage.To.Add(email);
        await Client.SendMailAsync(mailMessage);
    }
}