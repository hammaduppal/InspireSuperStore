using MimeKit;
using MailKit.Net.Smtp;
namespace MarketBal
{

    public class MailService
    {
        private readonly EmailSettings Mail_Settings = null;
        private readonly IConfiguration _config;
        public MailService(IConfiguration config)
        {
            _config = config;
            Mail_Settings = _config.GetSection("EmailSettings").Get<EmailSettings>();
        }
        public bool SendMail(MailData Mail_Data)
        {
            try
            {
                //MimeMessage - a class from Mimekit
                MimeMessage email_Message = new MimeMessage();
                MailboxAddress email_From = new MailboxAddress(Mail_Settings.SenderName, Mail_Settings.SenderEmail);
                email_Message.From.Add(email_From);
                MailboxAddress email_To = new MailboxAddress(Mail_Data.ToName, Mail_Data.ToEmail);
                email_Message.To.Add(email_To);
                email_Message.Subject = Mail_Data.EmailSubject;
                BodyBuilder emailBodyBuilder = new BodyBuilder();
                emailBodyBuilder.HtmlBody = Mail_Data.EmailBody;
                email_Message.Body = emailBodyBuilder.ToMessageBody();
                //this is the SmtpClient class from the Mailkit.Net.Smtp namespace, not the System.Net.Mail one
                SmtpClient MailClient = new SmtpClient();
                MailClient.Connect(Mail_Settings.Host, Mail_Settings.Port, Mail_Settings.EnableSsl);
                MailClient.Authenticate(Mail_Settings.SenderEmail, Mail_Settings.Password);
                MailClient.Send(email_Message);
                MailClient.Disconnect(true);
                MailClient.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                // Exception Details
                return false;
            }
        }
    }









    public class MailData
    {
        public string ToEmail { get; set; }
        public string ToName { get; set; }
        public string EmailSubject { get; set; }
        public string EmailBody { get; set; }
    }

    public class MailSettings
    {
    }

    public class EmailSettings
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public bool EnableSsl { get; set; }
        public bool UseDefaultCredentials { get; set; }
        public string SenderName { get; set; }
        public string SenderEmail { get; set; }
        public int Timeout { get; set; }
    }

}
