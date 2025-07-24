using MainModels.DTOModels;
using MainModels.Models;
using Microsoft.EntityFrameworkCore;

namespace InspireSuperStore.Areas.Notification.Data
{
    public class NotificationRepository
    {
        private readonly OneDb oneDb;
        public NotificationRepository(OneDb oneDb)
        {
            this.oneDb = oneDb;
        }
        public async Task<List<NotificationsDTO>> GetNotification(string userName)
        {
           return await  oneDb.Notifications.Where(x => x.UserId == userName).Select(x => new NotificationsDTO
            {
                UserId = x.UserId,
                CreatedAt = DateTime.Now,
                GroupName = x.GroupName,
                Id = x.Id,
                IsRead = x.IsRead,
                Message = x.Message,
                Params = x.Params
            }).ToListAsync();
        }
        public async Task<int> MarkAsRead(int Id)
        {
            var result = await oneDb.Notifications.Where(x => x.Id == Id).FirstOrDefaultAsync();
            result.IsRead = true;
          return await   oneDb.SaveChangesAsync();
          
        }
    }
}
