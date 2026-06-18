using MainModels;
using MainModels.DTOModels;
using MainModels.Models;
using MainModels.Util;
using MarketBal.Repository.InvoiceRP;
using MarketBal.Repository.Products;
using Microsoft.EntityFrameworkCore;

namespace MarketBal.Repository.IPM
{

    public class ProjectRepository
    {
        private readonly IConfiguration _config;
        private readonly DBManager _db;
        private readonly ApiMethods _api;
        private readonly OneDb _onedb;
        private readonly AttributeRepository _attrib;
        private readonly InvoiceRepository _invoiceRepository;
        public ProjectRepository(IConfiguration config, OneDb oneDb)
        {
            _config = config;
            _db = new DBManager(_config);
            _api = new ApiMethods();
            _onedb = oneDb;
            _attrib = new AttributeRepository(_config, _onedb);
            _invoiceRepository = new InvoiceRepository(_config, _onedb);
        }

        public async Task<List<ProjectVM>> GetUserBoards()
        {
            var userId = AppDataUtility.SessionUser.Id;
            var boards = await _onedb.Projects
    .Where(x => x.ProjectUsers.Any(pu => pu.UserId == userId && pu.IsActive == true) && x.IsActive == true)
    .Select(pb => new ProjectVM
    {
        ProjectId = pb.ProjectId,
        ProjectName = pb.ProjectName,

        // 1. Map Project Users
        ProjectUsers = pb.ProjectUsers
            .Where(pu => pu.IsActive == true && pu.IsDeleted == false)
            .Select(pu => new ProjectUserVM
            {
                ProjectUserId = pu.ProjectUserId,
                ProjectId = pu.ProjectId,
                UserId = pu.UserId,
                FirstName = pu.User.Person.FirstName,
                LastName = pu.User.Person.LastName,
                ImageUrl = pu.User.Person.ImageUrl,
            }).ToList(),

        // 2. FIXED: Map Columns and their Nested Tasks cleanly
        ProjectColumns = pb.ProjectColumns
            .Where(pc => pc.IsDeleted == false) // Filter out soft-deleted columns
            .OrderBy(pc => pc.SortOrder)        // Ensure board columns align correctly
            .Select(pc => new ProjectColumnVM
            {
                ColumnId = pc.ColumnId,
                ColumnName = pc.ColumnName,
                SortOrder = pc.SortOrder,

                // Nested Select Loop: Flattening and mapping tasks inside this column
                ProjectTasks = pc.ProjectTasks
                    .Where(pt => pt.IsDeleted == false) // Filter out soft-deleted tasks
                    .OrderBy(pt => pt.SortOrder)        // Keep task stack order intact
                    .Select(pt => new ProjectTaskVM
                    {
                        TaskId = pt.TaskId,
                        Title = pt.Title,
                        Priority = pt.Priority,
                        StartDate = pt.StartDate,
                        DueDate = pt.DueDate,
                        // Add any other specific card UI tracking fields you need here
                    }).ToList()
            }).ToList()
    }).ToListAsync();
            return boards;
        }
        public async Task<(bool Success, Guid NewId)> AddProject(ProjectVM model)
        {
            try
            {
                var newId = Guid.NewGuid();
                var project = new Project
                {
                    ProjectId = newId,
                    ProjectName = model.ProjectName,
                    BranchId = AppDataUtility.SessionUser.Person.Branch.BranchId,
                    StartDate = DateTime.UtcNow,
                    IsActive = true,
                    IsModified = false,
                    IsDeleted = false,
                    CreatedOn = DateTime.UtcNow
                };

                _onedb.Projects.Add(project);
                var defaultColumns = new List<ProjectColumn>
                {
                    new ProjectColumn
                    {
                        ColumnId = Guid.NewGuid(),
                        ProjectId = newId,
                        ColumnName = "Backlog",
                        SortOrder = 1,
                        IsActive = true,
                        IsModified = false,
                        IsDeleted = false,
                        CreatedOn = DateTime.UtcNow
                    },
                    new ProjectColumn
                    {
                        ColumnId = Guid.NewGuid(),
                        ProjectId = newId,
                        ColumnName = "In Progress",
                        SortOrder = 2,
                        IsActive = true,
                        IsModified = false,
                        IsDeleted = false,
                        CreatedOn = DateTime.UtcNow
                    },
                    new ProjectColumn
                    {
                        ColumnId = Guid.NewGuid(),
                        ProjectId = newId,
                        ColumnName = "In Review",
                        SortOrder = 3,
                        IsActive = true,
                        IsModified = false,
                        IsDeleted = false,
                        CreatedOn = DateTime.UtcNow
                    },
                    new ProjectColumn
                    {
                        ColumnId = Guid.NewGuid(),
                        ProjectId = newId,
                        ColumnName = "QA Testing",
                        SortOrder = 4,
                        IsActive = true,
                        IsModified = false,
                        IsDeleted = false,
                        CreatedOn = DateTime.UtcNow
                    },
                    new ProjectColumn
                    {
                        ColumnId = Guid.NewGuid(),
                        ProjectId = newId,
                        ColumnName = "Completed",
                        SortOrder = 5,
                        IsActive = true,
                        IsModified = false,
                        IsDeleted = false,
                        CreatedOn = DateTime.UtcNow
                    }
                };
                _onedb.ProjectColumns.AddRange(defaultColumns);
                var projectUser = new ProjectUser
                {
                    ProjectUserId = Guid.NewGuid(),
                    ProjectId = newId,
                    UserId = AppDataUtility.SessionUser.Id,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedOn = DateTime.UtcNow
                };

                _onedb.ProjectUsers.Add(projectUser);

                // Grouping SaveChangesAsync saves a database round-trip
                await _onedb.SaveChangesAsync();

                return (true, newId);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error adding project: {ex.Message}");
                return (false, Guid.Empty);
            }
        }

        public async Task<ProjectVM> GetProjectById(Guid projectId)
        {
            var boards = await _onedb.Projects.Where(x => x.ProjectId == projectId && x.IsActive == true).Select(pb => new ProjectVM
            {
                ProjectId = pb.ProjectId,
                ProjectName = pb.ProjectName,
                Description = pb.Description,
                ProjectCode = pb.ProjectCode,

                // 1. Map Project Team Users
                ProjectUsers = pb.ProjectUsers
                .Where(pu => pu.IsActive == true && pu.IsDeleted == false)
                .Select(pu => new ProjectUserVM
                {
                    ProjectUserId = pu.ProjectUserId,
                    ProjectId = pu.ProjectId,
                    UserId = pu.UserId,
                    FirstName = pu.User.Person.FirstName,
                    LastName = pu.User.Person.LastName,
                    ImageUrl = pu.User.Person.ImageUrl
                }).ToList(),

                ProjectColumns = pb.ProjectColumns
                .Where(pc => pc.IsActive == true && pc.IsDeleted == false)
                .OrderBy(pc => pc.SortOrder)
                .Select(pc => new ProjectColumnVM
                {
                    ColumnId = pc.ColumnId,
                    ColumnName = pc.ColumnName,
                    SortOrder = pc.SortOrder,

                    ProjectTasks = pc.ProjectTasks
                        .Where(pt => pt.IsActive == true && pt.IsDeleted == false)
                        .OrderBy(pt => pt.SortOrder)
                        .Select(pt => new ProjectTaskVM
                        {
                            TaskId = pt.TaskId,
                            ColumnId = pt.ColumnId,
                            Title = pt.Title,
                            Description = pt.Description,
                            TaskNumber = pt.TaskNumber,
                            Priority = pt.Priority,
                            Status = pt.Status,
                            StartDate = pt.StartDate,
                            DueDate = pt.DueDate,
                            EstimatedHours = pt.EstimatedHours,
                            ActualHours = pt.ActualHours,
                            SortOrder = pt.SortOrder,
                            TaskAssignedUsers = pt.TaskAssignedUsers.Select(tau => new TaskAssignedUserVM
                            {
                                TaskAssignedUserId = tau.TaskAssignedUserId,
                                FirstName = tau.User.Person.FirstName,
                                LastName = tau.User.Person.LastName,
                                Email = tau.User.Person.Email,
                                ImageUrl = tau.User.Person.ImageUrl,


                            }).ToList(),
                        }).ToList()
                }).ToList()
            }).FirstOrDefaultAsync();
            return boards;
        }

        public async Task<ProjectTaskVM> GetProjectTask(Guid taskId)
        {
            var task = await _onedb.ProjectTasks.Where(x => x.TaskId == taskId && x.IsActive == true).Select(pt => new ProjectTaskVM
            {
                Title = pt.Title,
                Description = pt.Description,
                Priority = pt.Priority,
                DueDate = pt.DueDate,
                ColumnName = pt.Column.ColumnName,
                StartDate = pt.StartDate,
                CreatedOn = pt.CreatedOn,
                TaskId = pt.TaskId,
                TaskAssignedUsers = pt.TaskAssignedUsers.Select(tau => new TaskAssignedUserVM
                {
                    TaskAssignedUserId = tau.TaskAssignedUserId,
                    FirstName = tau.User.Person.FirstName,
                    LastName = tau.User.Person.LastName,
                    Email = tau.User.Person.Email,
                    ImageUrl = tau.User.Person.ImageUrl,
                }).ToList(),
                TaskComments = pt.TaskComments.Where(x => x.IsActive == true && x.IsDeleted == false).Select(tc => new TaskCommentVM
                {
                    TaskCommentId = tc.TaskCommentId,
                    TaskComments = tc.TaskComments,
                    UserId = tc.UserId,
                    CreatedOn = tc.CreatedOn,
                    IsActive = tc.IsActive,
                    IsDeleted = tc.IsDeleted
                }).ToList(),
                TaskAttachments = pt.TaskAttachments.Where(x => x.IsActive == true).Select(ta => new TaskAttachmentVM
                {
                    TaskAttachmentId = ta.TaskAttachmentId,
                    AttachmentUrl = ta.AttachmentUrl,
                    CreatedOn = ta.CreatedOn
                }).ToList(),
            }).FirstOrDefaultAsync();

            return task;
        }
        public async Task<List<ProjectReportVM>> GetProjectWiseReportAsync()
        {
            var currentDate = DateTime.UtcNow;

            return await _onedb.Projects
                .Where(p => p.IsActive == true && p.IsDeleted == false)
                .Select(p => new ProjectReportVM
                {
                    ProjectId = p.ProjectId,
                    ProjectName = p.ProjectName,

                    // Count total active tasks flattened across all columns
                    TotalTasks = p.ProjectColumns
                        .Where(c => c.IsDeleted == false)
                        .SelectMany(c => c.ProjectTasks)
                        .Count(t => t.IsDeleted == false),

                    // Count only tasks residing in "Done" or "Completed" columns
                    CompletedTasks = p.ProjectColumns
                        .Where(c => c.IsDeleted == false && (c.ColumnName.ToLower().Contains("done") || c.ColumnName.ToLower().Contains("complete")))
                        .SelectMany(c => c.ProjectTasks)
                        .Count(t => t.IsDeleted == false),

                    // Count tasks where DueDate has passed and are not in a completed column
                    OverdueTasks = p.ProjectColumns
                        .Where(c => c.IsDeleted == false && !c.ColumnName.ToLower().Contains("done") && !c.ColumnName.ToLower().Contains("complete"))
                        .SelectMany(c => c.ProjectTasks)
                        .Count(t => t.IsDeleted == false && t.DueDate < currentDate)
                })
                .ToListAsync();
        }

        public async Task<List<UserWorkloadReportVM>> GetUserWiseReportAsync()
        {
            // 1. Fetch real team member workload breakdowns
            var userReports = await _onedb.LoginUsers
                .Where(u => u.IsActive == true && u.ProjectUsers.Any(pu => pu.IsActive == true && pu.ProjectId != Guid.Empty))
                .Select(u => new UserWorkloadReportVM
                {
                    UserId = u.Id,
                    FullName = (u.Person.FirstName ?? "") + " " + (u.Person.LastName ?? ""),
                    ImageUrl = u.Person.ImageUrl,
                    IsUnassignedQueue = false,

                    BacklogCount = _onedb.ProjectTasks.Count(t => t.IsDeleted == false && t.TaskAssignedUsers.Any(tau => tau.UserId == u.Id) && t.Column.ColumnName.ToLower() == "backlog"),
                    InProgressCount = _onedb.ProjectTasks.Count(t => t.IsDeleted == false && t.TaskAssignedUsers.Any(tau => tau.UserId == u.Id) && t.Column.ColumnName.ToLower() == "in progress"),
                    ReviewCount = _onedb.ProjectTasks.Count(t => t.IsDeleted == false && t.TaskAssignedUsers.Any(tau => tau.UserId == u.Id) && t.Column.ColumnName.ToLower() == "in review"),
                    QACount = _onedb.ProjectTasks.Count(t => t.IsDeleted == false && t.TaskAssignedUsers.Any(tau => tau.UserId == u.Id) && (t.Column.ColumnName.ToLower() == "qa" || t.Column.ColumnName.ToLower() == "testing")),
                    CompletedCount = _onedb.ProjectTasks.Count(t => t.IsDeleted == false && t.TaskAssignedUsers.Any(tau => tau.UserId == u.Id) && t.Column.ColumnName.ToLower() == "completed")
                })
                .ToListAsync();

            // 2. Calculate tasks across the pipeline that completely lack user assignments
            var unassignedRow = new UserWorkloadReportVM
            {
                UserId = 0,
                FullName = "Unassigned Tasks Pool",
                ImageUrl = null,
                IsUnassignedQueue = true,

                // Check if TaskAssignedUsers is completely empty using !Any()
                BacklogCount = await _onedb.ProjectTasks.CountAsync(t => t.IsDeleted == false && !t.TaskAssignedUsers.Any() && t.Column.ColumnName.ToLower() == "backlog"),
                InProgressCount = await _onedb.ProjectTasks.CountAsync(t => t.IsDeleted == false && !t.TaskAssignedUsers.Any() && t.Column.ColumnName.ToLower() == "in progress"),
                ReviewCount = await _onedb.ProjectTasks.CountAsync(t => t.IsDeleted == false && !t.TaskAssignedUsers.Any() && t.Column.ColumnName.ToLower() == "in review"),
                QACount = await _onedb.ProjectTasks.CountAsync(t => t.IsDeleted == false && !t.TaskAssignedUsers.Any() && (t.Column.ColumnName.ToLower() == "qa" || t.Column.ColumnName.ToLower() == "testing")),
                CompletedCount = await _onedb.ProjectTasks.CountAsync(t => t.IsDeleted == false && !t.TaskAssignedUsers.Any() && t.Column.ColumnName.ToLower() == "completed")
            };

            // Append the row cleanly to the bottom of the list
            userReports.Add(unassignedRow);
            return userReports;
        }
      

    }

}
