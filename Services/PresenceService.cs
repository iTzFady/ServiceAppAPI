using System.Collections.Concurrent;

namespace ServiceApp.Services
{
    public class PresenceService
    {

            private readonly ConcurrentDictionary<string, string> _onlineUsers = new();

            public void AddUser(string userId, string connectionId)
            {
                _onlineUsers[userId] = connectionId;
            }

            public void RemoveUser(string userId)
            {
                _onlineUsers.TryRemove(userId, out _);
            }

            public bool IsUserConnected(string userId)
            {
                return _onlineUsers.ContainsKey(userId);
            }

            public string? GetConnectionId(string userId)
            {
                _onlineUsers.TryGetValue(userId, out var connectionId);
                return connectionId;
            }

            public IEnumerable<string> GetAllOnlineUsers()
            {
                return _onlineUsers.Keys;
            }
        }
}
