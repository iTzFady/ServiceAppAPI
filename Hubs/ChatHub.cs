using Microsoft.AspNetCore.SignalR;
using ServiceApp.Data;
using ServiceApp.Models;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using ServiceApp.Services;
namespace ServiceApp.Hubs
{
    public class ChatHub : Hub
    {
        private readonly AppDbContext _db;
        private readonly PushNotificationService _pushService;
        private readonly PresenceService _presenceService;

        public ChatHub(AppDbContext db, PresenceService presenceService, PushNotificationService pushService) {
            _db = db;
            _presenceService = presenceService;
            _pushService = pushService;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId))
            {
                _presenceService.AddUser(userId, Context.ConnectionId);
                await Clients.All.SendAsync("UserOnline", userId);
            }
        }
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId))
            {
                _presenceService.RemoveUser(userId);
                await Clients.All.SendAsync("UserOffline", userId);
            }
        }

        public async Task SendMessage(string senderUserId, string receiverUserId, string message , bool isImage = false)
        {
            var chatMessage = new ChatMessage
            {
                SenderId = senderUserId,
                ReceiverId = receiverUserId,
                Message = message,
                Timestamp = DateTime.Now,
                IsRead = false,
                isImage = isImage
            };
            _db.ChatMessages.Add(chatMessage);
            await _db.SaveChangesAsync();

            await Clients.Users(senderUserId, receiverUserId)
                .SendAsync("ReceiveMessage", senderUserId, receiverUserId, message, chatMessage.Id, isImage);


            var isReceiverOnline = _presenceService.IsUserConnected(receiverUserId);

            if (!isReceiverOnline) {
                var receiver = await _db.Users.FindAsync(Guid.Parse(receiverUserId));

                if (!string.IsNullOrEmpty(receiver?.ExpoPushToken))
                {
                    await _pushService.SendPushAsync(
                            token: receiver.ExpoPushToken,
                            title: "New Message",
                            body: message,
                            data: new { senderUserId }
                        );
                }


            }
        }
        public async Task MarkAsRead(string messageId)
        {
            var message = await _db.ChatMessages.FindAsync(Guid.Parse(messageId));
            if (message != null)
            {
                message.IsRead = true;
                await _db.SaveChangesAsync();

                await Clients.Users(message.SenderId.ToString() , message.ReceiverId.ToString()).SendAsync("MessageRead", messageId);
            }
        }
        public async Task Typing(string senderId , string receiverId) { 
            await Clients.User(receiverId).SendAsync("UserTyping", senderId);
        }
    }
}
