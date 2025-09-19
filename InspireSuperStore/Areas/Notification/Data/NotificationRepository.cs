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
        public async Task<List<NotificationsDTO>> GetNotification(int userName)
        {
           return await  oneDb.Notifications.Where(x => x.UserId == userName && x.IsRead).Select(x => new NotificationsDTO
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
        public async Task<int> SaveNotification(NotificationsDTO n)
        {
            try
            {
                if(string.IsNullOrEmpty(n.Message))
                {
                    n.Message = "New Notification";
                }
                var noti = new MainModels.Models.Notification
                {
                    UserId = n.UserId,
                    CreatedAt = DateTime.Now,
                    GroupName = n.GroupName,
                    IsRead = false,
                    Message = n.Message,
                    Params = n.Params
                };
                await oneDb.Notifications.AddAsync(noti);
                return await oneDb.SaveChangesAsync();
            }
            catch (Exception ex)
            {

                throw;
            }
           
        }
        public async Task<List<NotificationsDTO>> GetGroupNotification(string[] groups)
        {
            return await oneDb.Notifications.Where(x => groups.Contains(x.GroupName)).Select(x => new NotificationsDTO
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
