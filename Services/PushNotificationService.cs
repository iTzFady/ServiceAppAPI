using Microsoft.EntityFrameworkCore;
using ServiceApp.Data;
using ServiceApp.Models;
using ServiceApp.Models.Enums;
namespace ServiceApp.Services
{
    public class PushNotificationService
    {
        private readonly HttpClient _http;
        private readonly AppDbContext _db;

        public PushNotificationService(HttpClient http , AppDbContext db)
        {
            _http = http;
            _db = db;
        }

        public async Task<ExpoResponse?> SendPushAsync(string token, string title, string body, NotificationType type, object? data = null) {
            var payload = new
            {
                to = token,
                title,
                body,
                sound = "default",
                data = data ?? new { },
                priority = "high"
            };
            var url = "https://exp.host/--/api/v2/push/send";
            var response = await _http.PostAsJsonAsync(url, payload);
            if (!response.IsSuccessStatusCode) { 
                var text = await response.Content.ReadAsStringAsync();
                return null;
            }

            var notificationOwner = await _db.Users
                 .FirstOrDefaultAsync(r => r.ExpoPushToken == token );

            if ( notificationOwner == null ) return null;

            if (notificationOwner != null)
            {

                var notification = new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = notificationOwner.Id,
                    Title = title,
                    body = body,
                    Data = data != null ? System.Text.Json.JsonSerializer.Serialize(data) : null,
                    ReceivedAt = DateTime.Now,
                };
                _db.Notifications.Add(notification);

                await _db.SaveChangesAsync();
            }
            var json = await response.Content.ReadFromJsonAsync<ExpoResponse>();
            return json;
        }
    }

    public class ExpoResponse { public ExpoResponseData? data { get; set; } }
    public class ExpoResponseData
    {
        public string? Status { get; set; }
        public string? Id { get; set; }
        public string? Message { get; set; }
    }
}


