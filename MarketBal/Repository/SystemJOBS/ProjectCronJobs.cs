using MainModels.DTOModels;
using MainModels.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace MarketBal.Repository.SystemJOBS
{
    public class ProjectCronJobs
    {
        private readonly OneDb _oneDb;
        private readonly IConfiguration _config;
        public ProjectCronJobs(OneDb oneDb, IConfiguration config)
        {
            _oneDb = oneDb;
            _config = config;
        }

        public async Task<bool> SendEmail()
        {
            try
            {
                // Pulling 2 items as requested for cron-job iterations
                var emailsToSend = _oneDb.ProjectNotificationEmails
      .Include(x => x.User)
          .ThenInclude(u => u.Person)
      .Where(x => x.IsSent == false)
      .Take(2)
      .ToList();

                if (!emailsToSend.Any()) return true;

                var mailService = new MailService(_config);

                foreach (var item in emailsToSend)
                {
                    string emailBody = string.Empty;
                    string emailSubject = string.Empty;
                    string fullName = $"{item.User.Person.FirstName} {item.User.Person.LastName}";

                    switch ((ProjectNotificationType)item.ProjectEmailNotificationType)
                    {
                        case ProjectNotificationType.YouAreAddedToProject:
                            var addedData = JsonConvert.DeserializeObject<AddedToProjectData>(item.MessageJson);
                            emailSubject = $"You've been added to {addedData.ProjectName}";
                            emailBody = GenerateNotificationHTML(
                                fullName,
                                "Added to Project",
                                $"You have been added as a collaborator to the project board <strong>{addedData.ProjectName}</strong>. You can now view, track, and manage tasks.",
                                addedData.ProjectName,
                                addedData.DirectBoardUrl
                            );
                            break;

                        case ProjectNotificationType.SomeOneAddedToProject:
                            var someoneData = JsonConvert.DeserializeObject<SomeoneAddedData>(item.MessageJson);
                            emailSubject = $"New Team Member in {someoneData.ProjectName}";
                            emailBody = GenerateNotificationHTML(
                                fullName,
                                "New Team Member",
                                $"<strong>{someoneData.AddedPersonName}</strong> has been added as a collaborator to the project board <strong>{someoneData.ProjectName}</strong>.",
                                someoneData.ProjectName,
                                someoneData.DirectBoardUrl
                            );
                            break;

                        case ProjectNotificationType.NewTaskCreated:
                            var taskData = JsonConvert.DeserializeObject<NewTaskToBoard>(item.MessageJson);
                            emailSubject = $"New Task Created: {taskData.TaskName}";
                            emailBody = GenerateNotificationHTML(
                                fullName,
                                "New Task Created",
                                $"A new task <strong>\"{taskData.TaskName}\"</strong> has been created in the project board.",
                                taskData.ProjectName,
                                taskData.DirectBoardUrl
                            );
                            break;

                        case ProjectNotificationType.GenericTaskChanges:
                            var changeData = JsonConvert.DeserializeObject<GenericTaskChangesData>(item.MessageJson);
                            emailSubject = $"Task Updated in {changeData.ProjectName}";
                            emailBody = GenerateNotificationHTML(
                                fullName,
                                "Task Updated",
                                $"The task <strong>\"{changeData.TaskName}\"</strong> has been updated.<br/>Modifications: <em>{changeData.ChangeDetails}</em>",
                                changeData.ProjectName,
                                changeData.DirectBoardUrl
                            );
                            break;

                        default:
                            continue; // Skip unrecognized types
                    }

                    // Trigger actual mail delivery
                    mailService.SendMail(new MailData
                    {
                        EmailBody = emailBody,
                        ToEmail = item.User.Person.Email,
                        EmailSubject = emailSubject,
                        ToName = fullName
                    });

                    // Flag as processed
                    item.IsSent = true;
                }

                // Commit updates back to the DB
                await _oneDb.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Unified clean component framework for building HTML notification emails
        /// </summary>
        public string GenerateNotificationHTML(string recipientName, string headerTitle, string messageContent, string boardName, string boardUrl)
        {
            return $@"
        <style>
            body, table, td, a {{ text-size-adjust: 100%; -webkit-text-size-adjust: 100%; -ms-text-size-adjust: 100%; }}
            table, td {{ mso-table-lspace: 0pt; mso-table-rspace: 0pt; }}
            img {{ -ms-interpolation-mode: bicubic; border: 0; height: auto; line-height: 100%; outline: none; text-decoration: none; }}
            table {{ border-collapse: collapse !important; }}
            body {{ height: 100% !important; margin: 0 !important; padding: 0 !important; width: 100% !important; background-color: #f4f5f7; font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, Helvetica, Arial, sans-serif; }}
            @media screen and (max-width: 600px) {{
                .wrapper {{ width: 100% !important; max-width: 100% !important; }}
                .container {{ padding: 20px !important; }}
                .button {{ display: block !important; width: auto !important; text-align: center !important; }}
            }}
        </style> 
        <table border='0' cellpadding='0' cellspacing='0' width='100%' style='background-color: #f4f5f7; padding: 40px 0;'>
            <tr>
                <td align='center'>
                    <table border='0' cellpadding='0' cellspacing='0' width='100%' class='wrapper' style='max-width: 600px; background-color: #ffffff; border-radius: 8px; box-shadow: 0 4px 12px rgba(9, 30, 66, 0.08); overflow: hidden;'>
                        <tr>
                            <td align='left' style='background-color: #0052cc; padding: 32px 40px;'>
                                <h1 style='color: #ffffff; margin: 0; font-size: 24px; font-weight: 600; letter-spacing: -0.5px;'>{headerTitle}</h1>
                            </td>
                        </tr>
                        <tr>
                            <td class='container' style='padding: 40px;'>
                                <p style='margin: 0 0 16px 0; font-size: 16px; line-height: 24px; color: #172b4d;'>
                                    Hello <strong>{recipientName}</strong>,
                                </p>
                                <p style='margin: 0 0 24px 0; font-size: 16px; line-height: 24px; color: #172b4d;'>
                                    {messageContent}
                                </p>
                                <table border='0' cellpadding='0' cellspacing='0' width='100%' style='background-color: #fafbfc; border: 1px solid #dfe1e6; border-radius: 6px; margin-bottom: 32px;'>
                                    <tr>
                                        <td style='padding: 20px;'>
                                            <table border='0' cellpadding='0' cellspacing='0' width='100%'>
                                                <tr>
                                                    <td style='padding-bottom: 8px; font-size: 12px; font-weight: 700; color: #5e6c84; text-transform: uppercase; letter-spacing: 0.5px;'>Project Board</td>
                                                </tr>
                                                <tr>
                                                    <td style='font-size: 18px; font-weight: 600; color: #172b4d;'>{boardName}</td>
                                                </tr>
                                            </table>
                                        </td>
                                    </tr>
                                </table>
                                <table border='0' cellpadding='0' cellspacing='0' width='100%'>
                                    <tr>
                                        <td align='left' style='padding-bottom: 24px;'>
                                            <a href='{boardUrl}' target='_blank' class='button' style='background-color: #0052cc; color: #ffffff; text-decoration: none; padding: 12px 24px; font-size: 15px; font-weight: 600; border-radius: 4px; display: inline-block; box-shadow: 0 2px 4px rgba(0,0,0,0.1);'>Open Project Board</a>
                                        </td>
                                    </tr>
                                </table>
                                <p style='margin: 0; font-size: 14px; line-height: 20px; color: #5e6c84;'>
                                    If you think this was done in error, please contact your workspace administrator.
                                </p>
                            </td>
                        </tr>
                        <tr>
                            <td style='background-color: #f4f5f7; padding: 24px 40px; text-align: center; border-top: 1px solid #dfe1e6;'>
                                <p style='margin: 0 0 8px 0; font-size: 12px; color: #7a869a;'>
                                    Sent automatically by your Kanban Project Management System.
                                </p>
                                <p style='margin: 0; font-size: 12px; color: #7a869a;'>
                                    &copy; 2026 Management Portal. All rights reserved.
                                </p>
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
        </table>";
        }


    }

}
