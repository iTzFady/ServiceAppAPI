namespace ServiceApp.Services
{
    public class PushNotificationService
    {
        private readonly HttpClient _http;
        public PushNotificationService(HttpClient http) => _http = http;


        public async Task<ExpoResponse?> SendPushAsync(string token, string title, string body, object? data = null) {
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
                Console.WriteLine(text);
                return null;
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


