using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace ServiceApp.Hubs
{
    [Authorize]
    public class RequestsHub : Hub
    {
        public override async Task OnConnectedAsync() { 
            var userId = Context.User?.Identity?.Name;
            Console.WriteLine($"User connected: {userId}");
            await base.OnConnectedAsync();
        }
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier ?? Context.User?.Identity?.Name;
            if (!string.IsNullOrEmpty(userId))
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
            await base.OnDisconnectedAsync(exception);
        }
    }
}
