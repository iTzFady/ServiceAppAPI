using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace ServiceApp.Provider
{
    public class ConnectionUserIdProvider : IUserIdProvider
    {
        public string GetUserId(HubConnectionContext connection)
        {
            return connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? connection.User?.FindFirst("sub")?.Value
                ?? connection.User?.FindFirst("userId")?.Value
                ?? connection.User?.Identity?.Name;
        }
    }
}
