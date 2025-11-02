using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using ServiceApp.Services;
using ServiceApp.Data;

namespace ServiceApp.Hubs
{
    [Authorize]
    public class RequestsHub : Hub
    {
        private readonly PresenceService _presenceService;

        public RequestsHub(PresenceService presenceService)
        {
            _presenceService = presenceService;
        }

        public override async Task OnConnectedAsync() { 
            var userId = Context.User?.Identity?.Name;
            if (!string.IsNullOrEmpty(userId))
            {
                _presenceService.AddUser(userId, Context.ConnectionId);
                await Clients.All.SendAsync("UserOnline", userId);
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier ?? Context.User?.Identity?.Name;
            if (!string.IsNullOrEmpty(userId))
            {
                _presenceService.RemoveUser(userId);
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
                    await Clients.All.SendAsync("UserOffline", userId);
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}
