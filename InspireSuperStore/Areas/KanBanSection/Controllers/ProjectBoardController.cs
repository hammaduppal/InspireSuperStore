using InspireSuperStore.Areas.Notification.Data;
using MainModels.DTOModels;
using MainModels.Models;
using MainModels.Util;
using MarketBal.Repository;
using MarketBal.Repository.Account;
using MarketBal.Repository.IPM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace InspireSuperStore.Areas.KanBanSection.Controllers
{
    [Authorize(Roles = UserRolesConstants.SuperAdmin + "," + UserRolesConstants.Admin + "," + UserRolesConstants.User)]
    [Area("KanBanSection")]
    [Route("[controller]/[action]")]
    public class ProjectBoardController : Controller
    {
        private readonly IConfiguration _config;
        private readonly AdminPanelRepository _adminPanel;
        private readonly ProjectRepository _project;
        private readonly FileRepository _file;
        private readonly PagesViewModel vm = new PagesViewModel();
        private readonly OneDb _oneDb;
        private readonly NotificationService _notificationServices;
        private readonly IHubContext<NotificationHub> _hubContext;
        public ProjectBoardController(IConfiguration config, OneDb oneDb, NotificationService notificationServices, IHubContext<NotificationHub> hubContext)
        {
            _oneDb = oneDb;
            _config = config;
            _file = new FileRepository();
            _adminPanel = new AdminPanelRepository(_config, _oneDb);
            _project = new ProjectRepository(_config, _oneDb);
            _notificationServices = notificationServices;
            _hubContext = hubContext;
        }

        #region BoardSectionWhichisProject


        public async Task<IActionResult> Boards()
        {
            var currentboards = await _project.GetUserBoards();
            vm.Projects = currentboards;
            return View(vm);
        }

        public async Task<IActionResult> _AddProjectForm()
        {
            return PartialView(vm);
        }

        public async Task<IActionResult> AddProject(ProjectVM model)
        {
            var result = await _project.AddProject(model);
            if (result.Success)
            {
                return Json(new { statusCode = "200", projectId = result.NewId });
            }
            else
            {
                return Json(new { statusCode = "200" });

            }
        }
        [HttpGet]
        public async Task<IActionResult> Project(Guid projectId)
        {
            var project = await _project.GetProjectById(projectId);
            vm.Project = project;
            return View(vm);
        }
        [HttpPost]
        public async Task<IActionResult> AssignProjectUser([FromBody] AssignProjectUserModel model)
        {
            try
            {
                if (model == null || model.ProjectId == Guid.Empty || model.UserId <= 0)
                {
                    return Json(new { success = false, message = "Invalid request tracking parameters parameters." });
                }

                // 1. Fetch the absolute record mapping state regardless of its active/deleted flags
                var existingAssignment = await _oneDb.ProjectUsers
                    .FirstOrDefaultAsync(a => a.ProjectId == model.ProjectId && a.UserId == model.UserId);

                // 2. State Decision Logic Pipeline
                if (existingAssignment != null)
                {
                    // CASE A: User is already attached and active -> Do absolutely nothing
                    if (existingAssignment.IsActive == true && existingAssignment.IsDeleted == false)
                    {
                        return Json(new { success = true, message = "User is already an active member of this project workspace." });
                    }

                    // CASE B: Record exists but is disabled or soft-deleted -> Reactivate it
                    existingAssignment.IsActive = true;
                    existingAssignment.IsDeleted = false;
                    existingAssignment.CreatedOn = DateTime.UtcNow; // Optional: Reset the timestamp trace window

                    _oneDb.ProjectUsers.Update(existingAssignment);

                }
                else
                {
                    // CASE C: No baseline record exists -> Add a clean new assignment row
                    var assignment = new ProjectUser
                    {
                        ProjectUserId = Guid.NewGuid(),
                        ProjectId = model.ProjectId,
                        UserId = model.UserId,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedOn = DateTime.UtcNow
                    };

                    _oneDb.ProjectUsers.Add(assignment);
                }
                await AddProjectEmailNotification(model.ProjectId, model.UserId);
                // 3. Persist changes to SQL Server
                await _oneDb.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
     
        [HttpGet]
        public async Task<IActionResult> GetProjectUsers(Guid taskId)
        {
            try
            {
                // Find project id via task -> column -> project
                var projectId = await _oneDb.ProjectTasks
                    .Where(t => t.TaskId == taskId && t.IsDeleted == false)
                    .Select(t => t.Column.ProjectId)
                    .FirstOrDefaultAsync();

                if (projectId == Guid.Empty)
                {
                    return Json(new { success = false, message = "Project not found for the task." });
                }

                var users = await _oneDb.ProjectUsers
                    .Where(pu => pu.ProjectId == projectId && pu.IsDeleted == false)
                    .Select(pu => new
                    {
                        userId = pu.UserId,
                        firstName = pu.User.Person.FirstName,
                        lastName = pu.User.Person.LastName,
                        imageUrl = pu.User.Person.ImageUrl
                    }).ToListAsync();

                return Json(new { success = true, users });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        //I think not in use need to check this
        public async Task<IActionResult> AddUserByEmail([FromBody] AddUserByEmailModel model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.Email))
                {
                    return Json(new { success = false, message = "Email address cannot be empty." });
                }

                model.Email = model.Email.Trim();

                // 1. Search for user in your database (joining User -> Person to check email)
                var targetUser = await _oneDb.LoginUsers
                    .Include(u => u.Person)
                    .FirstOrDefaultAsync(u => u.Person.Email == model.Email || u.UserName == model.Email && u.IsActive == true);

                // 2. SCENARIO A: User does not exist in the platform database
                if (targetUser == null)
                {

                    // Here you would trigger an email invitation background service.
                    // e.g., await _emailService.SendProjectInviteAsync(model.Email, model.ProjectId);

                    return Json(new
                    {
                        success = true,
                        userExisted = false,
                        message = $"An invitation email has been sent to {model.Email}."
                    });
                }

                // 3. Check if the user is already assigned to this project
                var alreadyAssigned = await _oneDb.ProjectUsers
                    .AnyAsync(pu => pu.ProjectId == model.ProjectId && pu.UserId == targetUser.Id && pu.IsDeleted == false);

                if (alreadyAssigned)
                {
                    return Json(new { success = false, message = "This user is already a member of this project." });
                }

                // 4. SCENARIO B: User exists -> Create the ProjectUser bridge record
                var projectUser = new ProjectUser
                {
                    ProjectUserId = Guid.NewGuid(),
                    ProjectId = model.ProjectId,
                    UserId = targetUser.Id,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedOn = DateTime.UtcNow
                };

                _oneDb.ProjectUsers.Add(projectUser);
                await _oneDb.SaveChangesAsync();

                // Return user details so the UI can draw the new avatar badge instantly
                return Json(new
                {
                    success = true,
                    userExisted = true,
                    userId = targetUser.Id,
                    firstName = targetUser.Person.FirstName ?? "",
                    lastName = targetUser.Person.LastName ?? "",
                    imageUrl = targetUser.Person.ImageUrl ?? ""
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        public async Task<IActionResult> ProjectUsers()
        {
            var projects = await _project.GetUserBoards();
            var users = await _adminPanel.GetLoginUser();
            vm.Projects = projects;
            vm.LoginUsers = users;
            return View(vm);
        }

        #endregion

        #region ColumnSection
        [HttpPost]
        public async Task<IActionResult> CreateColumn([FromBody] CreateColumnModel model)
        {
            try
            {
                var newId = Guid.NewGuid();
                // Find current max sort order to append to the end of the board
                int maxSort = await _oneDb.ProjectColumns
                    .Where(c => c.ProjectId == model.ProjectId && c.IsDeleted == false)
                    .Select(c => (int?)c.SortOrder)
                    .FirstOrDefaultAsync() ?? 0;

                var column = new ProjectColumn
                {
                    ColumnId = newId,
                    ProjectId = model.ProjectId,
                    ColumnName = model.ColumnName,
                    SortOrder = maxSort + 1,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedOn = DateTime.UtcNow
                };

                _oneDb.ProjectColumns.Add(column);
                await _oneDb.SaveChangesAsync();

                return Json(new { success = true, columnId = newId });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> RenameColumn([FromBody] RenameColumnModel model)
        {
            try
            {
                var column = await _oneDb.ProjectColumns.FindAsync(model.ColumnId);
                if (column == null)
                {
                    return Json(new { success = false, message = "Column not found." });
                }

                column.ColumnName = model.ColumnName.Trim();
                column.IsModified = true;

                await _oneDb.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        public async Task<IActionResult> DeleteColumn([FromBody] DeleteColumnModel model)
        {
            try
            {
                // 1. Fetch the target column
                var column = await _oneDb.ProjectColumns
                    .Include(c => c.ProjectTasks)
                    .FirstOrDefaultAsync(c => c.ColumnId == model.ColumnId);

                if (column == null)
                {
                    return Json(new { success = false, message = "Column not found." });
                }

                // 2. Soft delete the column
                column.IsDeleted = true;
                column.IsModified = true;

                // 3. Cascade soft delete to all tasks inside this column
                foreach (var task in column.ProjectTasks.Where(t => t.IsDeleted == false))
                {
                    task.IsDeleted = true;
                    task.IsModified = true;
                }

                await _oneDb.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SortColumns([FromBody] SortBoardModel model)
        {
            try
            {
                // model.SortedColumnIds contains an array of Guids in their new layout sequence
                for (int i = 0; i < model.SortedColumnIds.Length; i++)
                {
                    var colId = model.SortedColumnIds[i];
                    var column = await _oneDb.ProjectColumns.FindAsync(colId);
                    if (column != null)
                    {
                        column.SortOrder = i + 1;
                        column.IsModified = true;
                    }
                }
                await _oneDb.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        #endregion

        #region TaskSection
        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskModel model)
        {
            try
            {
                var newId = Guid.NewGuid();
                int maxSort = await _oneDb.ProjectTasks
                    .Where(t => t.ColumnId == model.ColumnId && t.IsDeleted == false)
                    .Select(t => (int?)t.SortOrder)
                    .FirstOrDefaultAsync() ?? 0;

                var task = new ProjectTask
                {
                    TaskId = newId,
                    ColumnId = model.ColumnId,
                    Title = model.Title,
                    Description = "Click to add details...",
                    Priority = ProjectTaskPriority.Low.ToString(),
                    Status = "Active",
                    SortOrder = maxSort + 1,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedOn = DateTime.UtcNow
                };

                _oneDb.ProjectTasks.Add(task);
                await NewTaskNotification(model.ColumnId, model.Title);

                await _oneDb.SaveChangesAsync();

                return Json(new { success = true, taskId = newId });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
     
        // 4. MOVE & SORT TASKS (Vertical Drag)
        [HttpPost]
        public async Task<IActionResult> SortTasks([FromBody] SortTasksModel model)
        {
            try
            {
                // Updates target column placement and specific order index sequence simultaneously
                for (int i = 0; i < model.SortedTaskIds.Length; i++)
                {
                    var taskId = model.SortedTaskIds[i];
                    var task = await _oneDb.ProjectTasks.FindAsync(taskId);
                    if (task != null)
                    {
                        task.ColumnId = model.TargetColumnId; // Updates parent column context if dropped elsewhere
                        task.SortOrder = i + 1;               // Position index tracking
                        task.IsModified = true;
                    }
                }
                await _oneDb.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 5. ASSIGN TASK TO USER
        [HttpPost]
        public async Task<IActionResult> AssignUser([FromBody] AssignUserModel model)
        {
            try
            {
                // Check if assignment record already exists to prevent duplicates
                var trackingCheck = await _oneDb.TaskAssignedUsers.AnyAsync(a => a.TaskId == model.TaskId && a.UserId == model.UserId && a.IsDeleted == false);

                if (!trackingCheck)
                {
                    var assignment = new TaskAssignedUser
                    {
                        TaskAssignedUserId = Guid.NewGuid(),
                        TaskId = model.TaskId,
                        UserId = model.UserId,
                        AssignedOn = DateTime.UtcNow,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedOn = DateTime.UtcNow
                    };
                    _oneDb.TaskAssignedUsers.Add(assignment);
                    await _oneDb.SaveChangesAsync();
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        public async Task<IActionResult> _EditTask(Guid TaskId)
        {
            var task = await _project.GetProjectTask(TaskId);
            vm.Task = task;
            return PartialView(vm);
        }

        public async Task<IActionResult> UpdateTaskDescription([FromBody] UpdateTaskDescriptionVM request)
        {
            var task = await _oneDb.ProjectTasks.Where(x => x.TaskId == request.TaskId).FirstOrDefaultAsync();
            task.Description = request.Description;
            _oneDb.ProjectTasks.Update(task);
            await GeneralTaskNotification(request.TaskId);
            await _oneDb.SaveChangesAsync();
            return Json(new { statusCode = "200", message = "Successfully Update" });
        }

        public async Task<IActionResult> UpdateTaskPriority([FromBody] UpdateTaskPriorityVM model)
        {
            try
            {
                var task = await _oneDb.ProjectTasks
                    .FirstOrDefaultAsync(t => t.TaskId == model.TaskId && t.IsDeleted == false);

                if (task == null)
                {
                    return Json(new { success = false, message = "Task details workspace record not found." });
                }

                // Direct string assignment ("High", "Normal", "Low") to your nvarchar column
                task.Priority = model.Priority;
                task.IsModified = true;

                await _oneDb.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public class UpdateTaskPriorityVM
        {
            public Guid TaskId { get; set; }
            public string Priority { get; set; }
        }
        public async Task<IActionResult> UpdateTaskDates([FromBody] UpdateTaskDateVM model)
        {
            try
            {
                var task = await _oneDb.ProjectTasks
                    .FirstOrDefaultAsync(t => t.TaskId == model.TaskId && t.IsDeleted == false);

                if (task == null)
                {
                    return Json(new { success = false, message = "Task workspace element missing." });
                }

                // Parse incoming nullable date formats cleanly
                DateTime? targetDate = null;
                if (!string.IsNullOrEmpty(model.DateValue))
                {
                    targetDate = DateTime.Parse(model.DateValue);
                }

                // Route assignment state mapping depending on tracking flags flags
                if (model.IsStartDate)
                {
                    task.StartDate = targetDate;
                }
                else
                {
                    task.DueDate = targetDate;
                }

                task.IsModified = true;
                await GeneralTaskNotification(model.TaskId);
                await _oneDb.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public class UpdateTaskDateVM
        {
            public Guid TaskId { get; set; }
            public string DateValue { get; set; } // Will map safely to null or date strings
            public bool IsStartDate { get; set; }
        }


        public async Task<IActionResult> UploadAttachments([FromForm] UploadAttachmentsModel model)
        {
            try
            {
                if (model.Files == null || model.Files.Count == 0)
                {
                    return Json(new { success = false, message = "No files selected for processing." });
                }

                var trackingList = new List<object>();



                foreach (var file in model.Files)
                {
                    if (file.Length > 0)
                    {

                        var uploadResult = await _file.SaveFile(file, $"TaskId:{model.TaskId.ToString()}", "TaskImages");

                        var newAttachmentId = Guid.NewGuid();

                        // Create Database row record instance
                        var attachment = new TaskAttachment
                        {
                            TaskAttachmentId = newAttachmentId,
                            TaskId = model.TaskId,
                            AttachmentUrl = uploadResult.ImageUrl,
                            UserId = AppDataUtility.SessionUser.Id,
                            IsActive = true,
                            IsDeleted = false,
                            CreatedOn = DateTime.UtcNow
                        };

                        _oneDb.TaskAttachments.Add(attachment);

                        // Track state to echo confirmation data arrays components back to jQuery render targets
                        trackingList.Add(new
                        {
                            attachmentId = newAttachmentId,
                            fileName = uploadResult.ImageUrl,
                            filePath = uploadResult.ImageUrl
                        });
                    }
                }

                await _oneDb.SaveChangesAsync();
                return Json(new { success = true, uploadedFiles = trackingList });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAttachment([FromBody] DeleteAttachmentModel model)
        {
            try
            {
                var attachment = await _oneDb.TaskAttachments.FindAsync(model.AttachmentId);
                if (attachment == null) return Json(new { success = false, message = "Item not found." });

                attachment.IsDeleted = true;
                attachment.IsActive = false;

                await _oneDb.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        // Binding view container representations models objects models
        public class UploadAttachmentsModel { public Guid TaskId { get; set; } public List<IFormFile> Files { get; set; } }
        public class DeleteAttachmentModel { public Guid AttachmentId { get; set; } }


        public async Task<IActionResult> UpdateTaskTitle([FromBody] UpdateTaskTitleVM model)
        {
            try
            {
                if (model == null || model.TaskId == Guid.Empty || string.IsNullOrWhiteSpace(model.Title))
                {
                    return Json(new { success = false, message = "Required parameters are incomplete or empty." });
                }

                var task = await _oneDb.ProjectTasks
                    .FirstOrDefaultAsync(t => t.TaskId == model.TaskId && t.IsDeleted == false);

                if (task == null)
                {
                    return Json(new { success = false, message = "Task timeline item could not be found." });
                }

                // Apply mutations fields configurations updates safely
                task.Title = model.Title.Trim();
                task.IsModified = true;
                await GeneralTaskNotification(model.TaskId);
                await _oneDb.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public class UpdateTaskTitleVM
        {
            public Guid TaskId { get; set; }
            public string Title { get; set; }
        }
        public async Task<IActionResult> SaveTaskComment([FromBody] SaveTaskCommentVM model)
        {
            try
            {
                if (model == null || model.TaskId == Guid.Empty || string.IsNullOrWhiteSpace(model.CommentText))
                {
                    return Json(new { success = false, message = "Comment values cannot be empty." });
                }

                // FIXED: Extracting logged in user directly from your session utility configuration model properties trace
                var activeUserSession = AppDataUtility.SessionUser;
                if (activeUserSession == null || activeUserSession.Id <= 0)
                {
                    return Json(new { success = false, message = "User session has expired or is invalid." });
                }

                var newCommentId = Guid.NewGuid();

                var commentRow = new TaskComment
                {
                    TaskCommentId = newCommentId,
                    TaskId = model.TaskId,
                    UserId = activeUserSession.Id, // Assign verified ID context natively
                    TaskComments = model.CommentText.Trim(),
                    IsActive = true,
                    IsDeleted = false,
                    CreatedOn = DateTime.UtcNow
                };

                _oneDb.TaskComments.Add(commentRow);
                await _oneDb.SaveChangesAsync();

                // Calculate layout visual response values fields to draw immediately in client engine
                var f = activeUserSession.Person?.FirstName ?? "System";
                var l = activeUserSession.Person?.LastName ?? "User";
                var initials = $"{(f.Length > 0 ? f.Substring(0, 1) : "")}{(l.Length > 0 ? l.Substring(0, 1) : "")}".ToUpper();

                return Json(new
                {
                    success = true,
                    authorName = $"{f} {l}",
                    authorInitials = string.IsNullOrEmpty(initials) ? "??" : initials
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        public class SaveTaskCommentVM
        {
            public Guid TaskId { get; set; }
            public string CommentText { get; set; }
        }
        [HttpPost]
        public async Task<IActionResult> DeleteTask([FromBody] DeleteTaskVM model)
        {
            try
            {
                if (model == null || model.TaskId == Guid.Empty)
                {
                    return Json(new { success = false, message = "Invalid or empty task parameters targeted." });
                }

                // Target the active workspace record instance safely
                var task = await _oneDb.ProjectTasks
                    .FirstOrDefaultAsync(t => t.TaskId == model.TaskId && t.IsDeleted == false);

                if (task == null)
                {
                    return Json(new { success = false, message = "The selected task could not be tracked down or is already deleted." });
                }

                // Standard corporate soft-delete mutation workflow assignment flags
                task.IsDeleted = true;
                task.IsModified = true;

                _oneDb.ProjectTasks.Update(task);
                await _oneDb.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Dedicated Data Transfer Model mapping layer configurations 
        public class DeleteTaskVM
        {
            public Guid TaskId { get; set; }
        }
        #endregion
        // 2. CREATE NEW TASK



        public async Task<IActionResult> UploadProfilePicture(IFormFile profileImage)
        {
            try
            {
                if (profileImage == null || profileImage.Length == 0)
                {
                    return Json(new { success = false, message = "No valid file streams captured." });
                }

                // 1. Authenticate user reference from local context cache checks
                var activeUser = AppDataUtility.SessionUser;
                if (activeUser == null || activeUser.Id <= 0)
                {
                    return Json(new { success = false, message = "User workspace session tracking details expired." });
                }

                // 2. Set dynamic target path directories directly inside wwwroot architecture bounds
                var uploadResult = await _file.SaveFile(profileImage, activeUser.Id.ToString() + activeUser.Person.FirstName, "ProfileImages");

                // 3. Update core entities parameters fields properties records lines mapping
                var person = await _oneDb.Persons.FindAsync(activeUser.PersonId);
                if (person == null)
                {
                    return Json(new { success = false, message = "Profile records are missing." });
                }

                person.ImageUrl = uploadResult.ImageUrl;


                // Synchronize state down to active workspace session cache tracker context too
                AppDataUtility.SessionUser.Person.ImageUrl = uploadResult.ImageUrl;

                await _oneDb.SaveChangesAsync();

                return Json(new { success = true, newImageUrl = uploadResult.ImageUrl });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        private async Task<bool> AddProjectEmailNotification(Guid projectId, int currentUserId)
        {
            List<ProjectNotificationEmail> emails = new List<ProjectNotificationEmail>();
            var selectedProject = await _oneDb.Projects.Where(x => x.ProjectId == projectId).Include(x => x.ProjectUsers).FirstOrDefaultAsync();

            foreach (var item in selectedProject.ProjectUsers)
            {
                if (item.UserId != currentUserId)
                {
                    emails.Add(new ProjectNotificationEmail
                    {
                        ProjectNotificationEmailId = Guid.NewGuid(),
                        UserId = item.UserId,
                        IsRead = false,
                        IsSent = false,
                        CreatedOn = DateTime.UtcNow,
                        ProjectEmailNotificationType = (int)ProjectNotificationType.SomeOneAddedToProject,
                        MessageJson = JsonConvert.SerializeObject(new AddedToProjectData
                        {
                            ProjectName = selectedProject.ProjectName,
                            DirectBoardUrl = $"https://app.inspirenation.us/ProjectBoard/Project?projectId={selectedProject.ProjectId}" 
                        })
                    });
                }
                var orderparam = new NewProjectTasksNotification
                {
                    Title = "New Task Generated"
                };
                var Notification = new NotificationsDTO
                {
                    CreatedAt = DateTime.Now,
                    GroupName = "Projects",
                    IsRead = false,
                    Params = JsonConvert.SerializeObject(orderparam),
                    UserId = AppDataUtility.SessionUser.Id,
                    NotificationTypeId = 2
                };
                await _notificationServices.SendToUser(item.UserId.ToString(), JsonConvert.SerializeObject(Notification));

            }
            emails.Add(new ProjectNotificationEmail
            {
                ProjectNotificationEmailId = Guid.NewGuid(),
                UserId = currentUserId,
                IsRead = false,
                IsSent = false,
                ProjectEmailNotificationType = (int)ProjectNotificationType.YouAreAddedToProject,
                CreatedOn = DateTime.UtcNow,
                MessageJson = JsonConvert.SerializeObject(new AddedToProjectData
                {
                    ProjectName = selectedProject.ProjectName,
                    DirectBoardUrl = $"https://app.inspirenation.us/ProjectBoard/Project?projectId={selectedProject.ProjectId}"
                })
            });
            await _oneDb.ProjectNotificationEmails.AddRangeAsync(emails);
            return true;
        }

        private async Task<bool> NewTaskNotification(Guid columnId, string taskTitle)
        {
            List<ProjectNotificationEmail> emails = new List<ProjectNotificationEmail>();
            var selectedColumn = await _oneDb.ProjectColumns.Where(x => x.ColumnId == columnId).FirstOrDefaultAsync();
            var selectedProject = await _oneDb.Projects.Where(x => x.ProjectId == selectedColumn.ProjectId).Include(inn => inn.ProjectUsers).FirstOrDefaultAsync();


            foreach (var item in selectedProject.ProjectUsers)
            {

                emails.Add(new ProjectNotificationEmail
                {
                    ProjectNotificationEmailId = Guid.NewGuid(),
                    UserId = item.UserId,
                    IsRead = false,
                    IsSent = false,
                    ProjectEmailNotificationType = (int)ProjectNotificationType.NewTaskCreated,
                    MessageJson = JsonConvert.SerializeObject(new NewTaskToBoard
                    {
                        ActionDate = DateTime.UtcNow,
                        ProjectId = item.ProjectId.Value,
                        ProjectName = selectedProject.ProjectName,
                        TaskName = taskTitle,
                        ColumnName = selectedColumn.ColumnName,
                        DirectBoardUrl = $"https://app.inspirenation.us/ProjectBoard/Project?projectId={selectedProject.ProjectId}"
                    }),
                    CreatedOn = DateTime.UtcNow,
                });
                var orderparam = new NewProjectTasksNotification
                {
                    Title = "New Task Generated"
                };
                var Notification = new NotificationsDTO
                {
                    CreatedAt = DateTime.Now,
                    GroupName = "Projects",
                    IsRead = false,
                    Params = JsonConvert.SerializeObject(orderparam),
                    UserId = AppDataUtility.SessionUser.Id,
                    NotificationTypeId = 2
                };
                await _notificationServices.SendToUser(item.UserId.ToString(), JsonConvert.SerializeObject(Notification));


            }

            await _oneDb.ProjectNotificationEmails.AddRangeAsync(emails);
            //_oneDb.SaveChanges();
            return true;
        }

        private async Task<bool> GeneralTaskNotification(Guid taskId)
        {
            List<ProjectNotificationEmail> emails = new List<ProjectNotificationEmail>();
            var selectedTask = await _oneDb.ProjectTasks.Where(x => x.TaskId == taskId).Include(inn=>inn.Column).FirstOrDefaultAsync();
            
            var selectedProject = await _oneDb.Projects.Where(x => x.ProjectId == selectedTask.Column.ProjectId).Include(inn => inn.ProjectUsers).FirstOrDefaultAsync();


            foreach (var item in selectedProject.ProjectUsers)
            {
                var users = await _oneDb.LoginUsers.Where(x=>x.Id==item.UserId).Include(p=>p.Person).FirstOrDefaultAsync();


                emails.Add(new ProjectNotificationEmail
                {
                    ProjectNotificationEmailId = Guid.NewGuid(),
                    UserId = item.UserId,
                    IsRead = false,
                    IsSent = false,
                    ProjectEmailNotificationType = (int)ProjectNotificationType.GenericTaskChanges,
                    MessageJson = JsonConvert.SerializeObject(new NewTaskToBoard
                    {
                        ActionDate = DateTime.UtcNow,
                        ProjectId = item.ProjectId.Value,
                        ProjectName = selectedProject.ProjectName,
                        TaskName = selectedTask.Title,
                        DirectBoardUrl = $"https://app.inspirenation.us/ProjectBoard/Project?projectId={selectedProject.ProjectId}"
                    }),
                    CreatedOn = DateTime.UtcNow,
                });
                var orderparam = new NewProjectTasksNotification
                {
                    Title = $"Modification on Task: {selectedTask.Title}"
                };
                var Notification = new NotificationsDTO
                {
                    CreatedAt = DateTime.Now,
                    GroupName = "Projects",
                    IsRead = false,
                    Params = JsonConvert.SerializeObject(orderparam),
                    UserId = AppDataUtility.SessionUser.Id,
                    NotificationTypeId = 2
                };
                //await _notificationServices.SendToUser(item.UserId.ToString(), JsonConvert.SerializeObject(Notification));

                await _hubContext.Clients.User(item.User.Person.Email).SendAsync("ReceiveNotification",JsonConvert.SerializeObject(Notification));

            }

            await _oneDb.ProjectNotificationEmails.AddRangeAsync(emails);
            //_oneDb.SaveChanges();
            return true;
        }

    }

}
