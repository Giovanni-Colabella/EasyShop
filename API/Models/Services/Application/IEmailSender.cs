namespace API.Models.Services.Application;

public interface IEmailSender
{
    Task SendEmailAsync(string toEmail, string subject, string body);
    Task SendEmailWithAttachmentAsync(
        string toEmail,
        string subject,
        string body,
        byte[] attachmentBytes,
        string attachmentName
    );
}
