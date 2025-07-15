using System.Net;
using System.Net.Mail;
using ElasticEmailClient;
public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly HttpClient _httpClient;
    public EmailService(IConfiguration config)
    {

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
    public async Task SendEmailAsync(string toEmail, string subject, string htmlContent)
    {
        var apiKey = _config["ElasticEmail:ApiKey"];
        var from = _config["ElasticEmail:FromEmail"];
        var fromName = _config["ElasticEmail:FromName"];

        var values = new Dictionary<string, string>
        {
            { "apikey", apiKey },
            { "from", from },
            { "fromName", fromName },
            { "to", toEmail },
            { "subject", subject },
            { "bodyHtml", htmlContent },
            { "isTransactional", "true" }
        };

        var content = new FormUrlEncodedContent(values);

        var response = await _httpClient.PostAsync("https://api.elasticemail.com/v2/email/send", content);
        var result = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode || !result.Contains("\"success\":true"))
        {
            throw new Exception($"Elastic Email API Error: {result}");
        }
    }
}