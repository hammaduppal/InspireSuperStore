using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace InspireSuperStore.Areas.Notification.Data
{
    public class NotificationHub : Hub
    {

        private readonly NotificationService _notificationService;

        public NotificationHub(NotificationService notificationService)
        {
            _notificationService = notificationService;
        }
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                _notificationService.AddConnection(Context.ConnectionId, userId);

                // Add user to SignalR groups based on roles
                var roles = Context.User?.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
                if (roles != null)
                {
                    foreach (var role in roles)
                    {
                        await Groups.AddToGroupAsync(Context.ConnectionId, role);
                    }
                }

                await _notificationService.NotifyOnlineUsersUpdated();
            }

            await base.OnConnectedAsync();
        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            _notificationService.RemoveConnection(Context.ConnectionId);
            await _notificationService.NotifyOnlineUsersUpdated();

            await base.OnDisconnectedAsync(exception);
        }
     
        public async Task SendNotification(string message)
        {
            await Clients.All.SendAsync("ReceiveNotification", message);
        }
    }
}
