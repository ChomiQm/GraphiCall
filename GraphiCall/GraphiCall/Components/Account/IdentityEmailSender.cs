using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using GraphiCall.Data;
using GraphiCall.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace GraphiCall.Components.Account
{
    public class IdentityEmailSender : IEmailSender<ApplicationUser>
    {
        private readonly SmtpSettings _smtpSettings;
        private readonly ILogger<IdentityEmailSender> _logger;

        public IdentityEmailSender(IOptions<SmtpSettings> smtpSettings, ILogger<IdentityEmailSender> logger)
        {
            _smtpSettings = smtpSettings.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                using var smtpClient = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port)
                {
                    Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password),
                    EnableSsl = _smtpSettings.EnableSSL
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_smtpSettings.FromEmail),
                    Subject = subject,
                    Body = htmlMessage,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(email);

                await smtpClient.SendMailAsync(mailMessage);
            }
            catch (SmtpException smtpEx)
            {
                _logger.LogError(smtpEx, "SMTP error occurred when sending email.");
                throw; // Rethrow the exception, you could also return a result indicating failure.
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred when sending email.");
                throw new InvalidOperationException("An unexpected problem occurred when sending an email.", ex);
            }
        }

        public async Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
        {
            try
            {
                var subject = "Confirm your email address";
                var htmlMessage = $"Please activate your account by clicking in this link <a href='{confirmationLink}' >here</a>.";
                await SendEmailAsync(email, subject, htmlMessage);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Cant send confirmation link.", ex);
            }
        }

        public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
        {
            try
            {
                var subject = "Reset password";
                var htmlMessage = $"Please reset your password by clicking in this link <a href='{resetLink}'> here</a>.";
                return SendEmailAsync(email, subject, htmlMessage);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Cant send reset password link.", ex);
            }
        }

        public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
        {
            try
            {
                var subject = "Reset password code";
                var htmlMessage = $"Your reset password code: {resetCode}";
                return SendEmailAsync(email, subject, htmlMessage);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Cant send reset password code link.", ex);
            }
        }
    }
}
