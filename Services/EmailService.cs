using System.Net;
using System.Net.Mail;
using ElasticEmailClient;
public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly HttpClient _httpClient;
    public EmailService(IConfiguration config, HttpClient httpClient)
    {
        _config = config;
        _httpClient = httpClient;
    }
    // public async Task SendEmailAsync(string email, string subject, string message)
    // {

    //     var host = Environment.GetEnvironmentVariable("EMAIL_HOST");
    //     var port = int.Parse(Environment.GetEnvironmentVariable("EMAIL_PORT"));
    //     var username = Environment.GetEnvironmentVariable("EMAIL_USERNAME");
    //     var password = Environment.GetEnvironmentVariable("EMAIL_PASSWORD");
    //     var fromEmail = Environment.GetEnvironmentVariable("EMAIL_FROM");
    //     var fromName = Environment.GetEnvironmentVariable("EMAIL_FROM_NAME");
    //     using var Client = new SmtpClient(host, port)
    //     {
    //         Credentials = new NetworkCredential(username, password),
    //         EnableSsl = true
    //     };
    //     var mailMessage = new MailMessage
    //     {
    //         From = new MailAddress(fromEmail, fromName),
    //         Subject = subject,
    //         Body = message,
    //         IsBodyHtml = true
    //     };
    //     mailMessage.To.Add(email);
    //     await Client.SendMailAsync(mailMessage);
    // }
    public async Task SendEmailAsync(string to, string subject, string html)
    {
        var apiKey = _config["ElasticEmail:ApiKey"];
        var from = _config["ElasticEmail:FromEmail"];
        var fromName = _config["ElasticEmail:FromName"];

        var payload = new
        {
            from,
            to,
            subject,
            html
        };

        var req = new HttpRequestMessage(HttpMethod.Post, "https://api.resend.com/emails");
        req.Headers.Add("Authorization", $"Bearer {apiKey}");
        req.Content = JsonContent.Create(payload);

        var res = await _httpClient.SendAsync(req);
        var result = await res.Content.ReadAsStringAsync();

        if (!res.IsSuccessStatusCode)
            throw new Exception($"Resend failed: {result}");
    }
}