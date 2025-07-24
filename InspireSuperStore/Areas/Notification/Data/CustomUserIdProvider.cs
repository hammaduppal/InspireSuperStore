using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace InspireSuperStore.Areas.Notification.Data
{
    public class CustomUserIdProvider : IUserIdProvider
    {
        public string GetUserId(HubConnectionContext connection)
        {
            // Use NameIdentifier or Email, depending on how you identify your users
            return connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
