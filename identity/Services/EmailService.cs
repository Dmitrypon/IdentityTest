using MailKit;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Palantir.Identity.Configuration;
using System;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading.Tasks;
using System.Web;

namespace Palantir.Identity.Services
{
    public sealed class EmailService : IEmailSender
    {
        private EmailRelay _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailRelay> config, ILogger<EmailService> logger)
        {
            _config = config.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            #region formatter
            //string text = string.Format("Please click on this link to {0}: {1}", subject, message.Body);
            //string html = "Please confirm your account by clicking this link: <a href=\"" + message.Body + "\">link</a><br/>";

            //html += HttpUtility.HtmlEncode(@"Or click on the copy the following link on the browser:" + message.Body);
            #endregion

            var message = new MimeMessage
            {
                Subject = subject
            };
            message.From.Add(new MailboxAddress(_config.From, _config.From));
            message.To.Add(new MailboxAddress(email, email));
            //msg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(text, null, MediaTypeNames.Text.Plain));
            message.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = htmlMessage
            };

            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                await client.ConnectAsync(_config.Server, _config.Port, _config.UseSSL);

                if (!string.IsNullOrWhiteSpace(_config.Login))
                {
                    await client.AuthenticateAsync(_config.Login, _config.Password);
                }

                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }
    }
}
