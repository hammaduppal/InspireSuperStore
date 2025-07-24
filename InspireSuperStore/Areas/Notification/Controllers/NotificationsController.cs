using InspireSuperStore.Areas.Notification.Data;
using MainModels.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InspireSuperStore.Areas.Notification.Controllers
{
    public class NotificationsController : Controller
    {
        //private readonly NotificationRepository _notificationRepository;
        private readonly OneDb _oneDb;
        public NotificationsController(OneDb oneDb)
        {
            _oneDb = oneDb;
            //_notificationRepository =new NotificationRepository(_oneDb);
        }
        [HttpGet("notifications/getNotifications/{userId}")]
        public async Task<IActionResult> GetUserNotifications(string userId)
        {
           // var notifications = await _notificationRepository.GetNotification(userId);

            return Ok();
        }
        [HttpPost("notifications/mark-read/{id}")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
           // var notif = await _notificationRepository.MarkAsRead(id);

            return Ok();
        }
    }
}
