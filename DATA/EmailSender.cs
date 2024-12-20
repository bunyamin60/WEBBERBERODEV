using Microsoft.AspNetCore.Identity.UI.Services;

namespace WEBBERBERODEV.DATA
{
    public class EmailSender : IEmailSender
    {
        Task IEmailSender.SendEmailAsync(string email, string subject, string htmlMessage)
        {
           //Email gönderme işlemlerini buraya  yapabilirsiniz.
            return Task.CompletedTask;
        }
    }
}
