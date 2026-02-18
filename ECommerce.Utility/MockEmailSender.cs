using System.Threading.Tasks;
using ECommerce.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace ECommerce.Utility
{
    /// <summary>
    /// Geliştirme ortamı için basit mock e-posta servisi.
    /// Gerçek SMTP yerine log'a yazar.
    /// </summary>
    public class MockEmailSender : IEmailSender<ApplicationUser>
    {
        private readonly ILogger<MockEmailSender> _logger;

        public MockEmailSender(ILogger<MockEmailSender> logger)
        {
            _logger = logger;
        }

        public Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            _logger.LogInformation("📧 Mock email to {Email} - {Subject}\n{Body}",
                toEmail, subject, htmlBody);
            return Task.CompletedTask;
        }

        public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
        {
            _logger.LogInformation("✉️ Confirmation link for {User} ({Email}): {Link}",
                user.FullName, email, confirmationLink);
            return Task.CompletedTask;
        }

        public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
        {
            _logger.LogInformation("🔐 Password reset link for {User} ({Email}): {Link}",
                user.FullName, email, resetLink);
            return Task.CompletedTask;
        }

        public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
        {
            _logger.LogInformation("🔑 Password reset code for {User} ({Email}): {Code}",
                user.FullName, email, resetCode);
            return Task.CompletedTask;
        }
    }
}


