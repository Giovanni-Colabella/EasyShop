using API.Models.Options;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;

using MimeKit;

namespace API.Models.Services.Application;

public class EmailSender : IEmailSender
{
    private readonly IConfiguration _config;
    private readonly SmtpSettings _smtpSettings;
    public EmailSender(IConfiguration config, IOptionsMonitor<SmtpSettings> smtpSettings)
    {
        _config = config;
        _smtpSettings = smtpSettings.CurrentValue;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_smtpSettings.SenderName, _smtpSettings.SenderEmail));
        message.To.Add(new MailboxAddress("", toEmail));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = body
        };
        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        try 
        {
            await client.ConnectAsync(_smtpSettings.Server, _smtpSettings.Port, _smtpSettings.EnableSsl);
            await client.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password);
            await client.SendAsync(message);
        } catch (Exception ex) 
        {
            throw new Exception($"Errore durante l'invio dell'email: {ex.Message}", ex);
        } finally 
        {
            await client.DisconnectAsync(true);
        }
    }

    public async Task SendEmailWithAttachmentAsync(
        string toEmail,
        string subject,
        string body,
        byte[] attachmentBytes,
        string attachmentName)
    {
        // Crea il messaggio
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_smtpSettings.SenderName, _smtpSettings.SenderEmail));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;

        // Costruisci il corpo HTML e aggiungi l’allegato
        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = body
        };

        if (attachmentBytes != null && attachmentBytes.Length > 0)
        {
            // Aggiunge l’allegato col nome file desiderato
            bodyBuilder.Attachments.Add(
                attachmentName,
                new MemoryStream(attachmentBytes),
                new ContentType("application", "pdf"));
        }

        message.Body = bodyBuilder.ToMessageBody();

        // Invia tramite SMTP
        using var client = new SmtpClient();
        try
        {
            await client.ConnectAsync(_smtpSettings.Server, _smtpSettings.Port, _smtpSettings.EnableSsl);
            await client.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password);
            await client.SendAsync(message);
        }
        catch (Exception ex)
        {
            throw new Exception($"Errore durante l'invio dell'email: {ex.Message}", ex);
        }
        finally
        {
            await client.DisconnectAsync(true);
        }
    }
}
