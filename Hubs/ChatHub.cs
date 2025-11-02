using Microsoft.AspNetCore.SignalR;
using ServiceApp.Data;
using ServiceApp.Models;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using ServiceApp.Services;
using Microsoft.AspNetCore.Authorization;
using ServiceApp.Models.Enums;
namespace ServiceApp.Hubs
{
    [Authorize]
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

        public async Task SendMessage(Guid senderUserId, Guid receiverUserId, string message , bool isImage = false)
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

            await Clients.Users(senderUserId.ToString(), receiverUserId.ToString())
                .SendAsync("ReceiveMessage", senderUserId, receiverUserId, message, chatMessage.Id, isImage);


            var isReceiverOnline = _presenceService.IsUserConnected(receiverUserId.ToString());

            if (!isReceiverOnline) {
                var receiver = await _db.Users.FindAsync(Guid.Parse(receiverUserId.ToString()));
                var sender = await _db.Users.FindAsync(senderUserId);

                if (!string.IsNullOrEmpty(receiver?.ExpoPushToken) && sender !=null)
                {
                    await _pushService.SendPushAsync(
                            token: receiver.ExpoPushToken,
                            title: $"رسالة جديدة من {sender.Name}",
                            body: isImage ? "أرسل لك صورة" : message,
                            data: new { senderUserId },
                            type: NotificationType.Message
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
