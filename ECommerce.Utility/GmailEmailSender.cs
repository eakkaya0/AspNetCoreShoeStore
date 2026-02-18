using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using ECommerce.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ECommerce.Utility
{
    /// <summary>
    /// Gmail SMTP kullanarak e-posta gönderimi sağlayan servis.
    /// </summary>
    public class GmailEmailSender : IEmailSender<ApplicationUser>
    {
        private readonly ILogger<GmailEmailSender> _logger;
        private readonly string _senderEmail;
        private readonly string _senderPassword;

        public GmailEmailSender(IConfiguration configuration, ILogger<GmailEmailSender> logger)
        {
            _logger = logger;
            _senderEmail = configuration["EmailSettings:SenderEmail"] 
                ?? throw new InvalidOperationException("EmailSettings:SenderEmail is not configured");
            _senderPassword = configuration["EmailSettings:SenderPassword"] 
                ?? throw new InvalidOperationException("EmailSettings:SenderPassword is not configured");
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            try
            {
                using (var smtpClient = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.Credentials = new NetworkCredential(_senderEmail, _senderPassword);
                    smtpClient.EnableSsl = true;
                    smtpClient.Timeout = 10000;

                    using (var mailMessage = new MailMessage())
                    {
                        mailMessage.From = new MailAddress(_senderEmail, "E-Commerce Uygulaması");
                        mailMessage.To.Add(toEmail);
                        mailMessage.Subject = subject;
                        mailMessage.Body = htmlBody;
                        mailMessage.IsBodyHtml = true;

                        await smtpClient.SendMailAsync(mailMessage);
                        _logger.LogInformation("E-posta başarıyla gönderildi: {ToEmail} - {Subject}", toEmail, subject);
                    }
                }
            }
            catch (SmtpException smtpEx)
            {
                _logger.LogError($"SMTP hatası ({toEmail}): {smtpEx.Message}. Status Code: {smtpEx.StatusCode}");
                _logger.LogError($"Hata Detayı: {smtpEx.InnerException?.Message}");
                _logger.LogError($"Gönderici E-posta: {_senderEmail}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"E-posta gönderimi başarısız ({toEmail}): {ex.Message}");
                throw;
            }
        }

        public async Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
        {
            var subject = "E-Mail Doğrulaması";
            var htmlBody = $"""
                <h2>E-Mail Doğrulaması</h2>
                <p>Merhaba {user.FirstName},</p>
                <p>E-Commerce hesabınızı etkinleştirmek için lütfen aşağıdaki bağlantıya tıklayınız:</p>
                <p><a href="{confirmationLink}">E-Mail Doğrulama Bağlantısı</a></p>
                <p>Bu bağlantı 24 saat boyunca geçerlidir.</p>
                <p>İyi alışverişler!</p>
                """;

            await SendEmailAsync(email, subject, htmlBody);
        }

        public async Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
        {
            var subject = "Şifre Sıfırlama";
            var htmlBody = $"""
                <h2>Şifre Sıfırlama</h2>
                <p>Merhaba {user.FirstName},</p>
                <p>Şifrenizi sıfırlamak için lütfen aşağıdaki bağlantıya tıklayınız:</p>
                <p><a href="{resetLink}">Şifre Sıfırlama Bağlantısı</a></p>
                <p>Bu bağlantı 1 saat boyunca geçerlidir.</p>
                <p>Eğer bu isteği siz yapmadıysanız, lütfen bu e-postayı dikkate almayınız.</p>
                """;

            await SendEmailAsync(email, subject, htmlBody);
        }

        public async Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
        {
            var subject = "Şifre Sıfırlama Kodu";
            var htmlBody = $"""
                <h2>Şifre Sıfırlama Kodu</h2>
                <p>Merhaba {user.FirstName},</p>
                <p>Şifrenizi sıfırlamak için aşağıdaki kodu kullanınız:</p>
                <p><strong style="font-size: 18px; letter-spacing: 2px;">{resetCode}</strong></p>
                <p>Bu kod 1 saat boyunca geçerlidir.</p>
                <p>Eğer bu isteği siz yapmadıysanız, lütfen bu e-postayı dikkate almayınız.</p>
                """;

            await SendEmailAsync(email, subject, htmlBody);
        }
    }
}

