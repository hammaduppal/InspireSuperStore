using MainModels.DTOModels;
using MainModels.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace InspireSuperStore.Areas.Notification.Data
{
    public class NotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly Dictionary<string, string> _connections = new();
        private readonly object _lock = new();
        public NotificationService(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }


        public void AddConnection(string connectionId, string userId)
        {
            lock (_lock)
            {
                _connections[connectionId] = userId;
            }
        }

        // Call this from NotificationHub.OnDisconnectedAsync
        public void RemoveConnection(string connectionId)
        {
            lock (_lock)
            {
                _connections.Remove(connectionId);
            }
        }

        public List<string> GetOnlineUsers()
        {
            lock (_lock)
            {
                return _connections.Values.Distinct().ToList();
            }
        }

        public async Task NotifyOnlineUsersUpdated()
        {
            var users = GetOnlineUsers();
            await _hubContext.Clients.All.SendAsync("UpdateOnlineUsers", users);
        }

        public async Task SendToUser(string userId, string message)
        {
            await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", message);
        }
        public async Task SendToAllUsers(string message)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", message);
        }

        public async Task SendToRoleGroup(string role, string message)
        {
            await _hubContext.Clients.Group(role).SendAsync("ReceiveNotification", message);
        }
    }
}
