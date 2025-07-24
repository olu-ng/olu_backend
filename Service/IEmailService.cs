
//using MailKit.Net.Smtp;
//using MailKit.Security;
//using MimeKit;
//using MimeKit.Text;
//using Microsoft.Extensions.Configuration;

//namespace OluBackendApp.Services
//{
//    public interface IEmailService
//    {
//        Task SendAsync(string to, string subject, string html);
//    }

//    public class EmailService : IEmailService
//    {
//        private readonly IConfiguration _cfg;
//        public EmailService(IConfiguration cfg) => _cfg = cfg;

//        public async Task SendAsync(string to, string subject, string html)
//        {
//            // 1) Basic argument guards
//            if (string.IsNullOrWhiteSpace(to))
//                throw new ArgumentException("Recipient email is required.", nameof(to));
//            html ??= string.Empty;
//            subject ??= string.Empty;

//            // 2) Pull your "Email" section
//            var section = _cfg.GetSection("Email");
//            var fromAddress = section["From"];
//            var fromDisplayName = section["DisplayName"];
//            var host = section["Host"];
//            var portRaw = section["Port"];
//            var user = section["Username"];
//            var pass = section["Password"];
//            var useSsl = bool.TryParse(section["UseSSL"], out var ssl) && ssl;
//            var startTls = bool.TryParse(section["EnableStartTls"], out var tls) && tls;

//            // 3) Validate config
//            if (string.IsNullOrWhiteSpace(fromAddress)
//             || string.IsNullOrWhiteSpace(host)
//             || !int.TryParse(portRaw, out var port)
//             || string.IsNullOrWhiteSpace(user)
//             || string.IsNullOrWhiteSpace(pass))
//            {
//                throw new InvalidOperationException("Missing or invalid Email configuration.");
//            }

//            // 4) Build the message
//            var message = new MimeMessage();
//            message.From.Add(new MailboxAddress(fromDisplayName ?? fromAddress, fromAddress));
//            try
//            {
//                message.To.Add(MailboxAddress.Parse(to));
//            }
//            catch (FormatException ex)
//            {
//                throw new ArgumentException($"'{to}' is not a valid email address.", nameof(to), ex);
//            }
//            message.Subject = subject;
//            message.Body = new TextPart(TextFormat.Html) { Text = html };

//            // 5) Choose the socket option
//            SecureSocketOptions option = useSsl
//                ? SecureSocketOptions.SslOnConnect    // port 465
//                : startTls
//                    ? SecureSocketOptions.StartTls    // common on 587
//                    : SecureSocketOptions.None;

//            // 6) Send
//            using var client = new SmtpClient();
//            await client.ConnectAsync(host, port, option);
//            await client.AuthenticateAsync(user, pass);
//            await client.SendAsync(message);
//            await client.DisconnectAsync(true);
//        }
//    }
//}



using System;
using System.Threading;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using Microsoft.Extensions.Configuration;

namespace OluBackendApp.Services
{
    public interface IEmailService
    {
        Task SendAsync(string to, string subject, string html);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _cfg;
        public EmailService(IConfiguration cfg) => _cfg = cfg;

        public async Task SendAsync(string to, string subject, string html)
        {
            // 1) Basic argument guards
            if (string.IsNullOrWhiteSpace(to))
                throw new ArgumentException("Recipient email is required.", nameof(to));
            subject ??= string.Empty;
            html ??= string.Empty;

            // 2) Load & validate config
            var section = _cfg.GetSection("Email");
            var fromAddr = section["From"];
            var fromName = section["DisplayName"];
            var host = section["Host"];
            var portRaw = section["Port"];
            var user = section["Username"];
            var pass = section["Password"];
            var useSsl = bool.TryParse(section["UseSSL"], out var ssl) && ssl;
            var startTls = bool.TryParse(section["EnableStartTls"], out var tls) && tls;

            if (string.IsNullOrWhiteSpace(fromAddr))
                throw new InvalidOperationException("Email:From is missing or empty.");
            if (string.IsNullOrWhiteSpace(host))
                throw new InvalidOperationException("Email:Host is missing or empty.");
            if (!int.TryParse(portRaw, out var port))
                throw new InvalidOperationException("Email:Port is missing or not a valid integer.");
            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
                throw new InvalidOperationException("Email:Username or Email:Password is missing or empty.");

            // 3) Parse sender address
            if (!MailboxAddress.TryParse($"{fromName} <{fromAddr}>", out var fromMailbox))
                throw new InvalidOperationException(
                    $"Invalid sender address: '{fromAddr}' with display name '{fromName}'.");

            // 4) Parse recipient address
            if (!MailboxAddress.TryParse(to, out var toMailbox))
                throw new ArgumentException($"'{to}' is not a valid email address.", nameof(to));

            // 5) Build the message
            var message = new MimeMessage();
            message.From.Add(fromMailbox);
            message.To.Add(toMailbox);
            message.Subject = subject;
            message.Body = new TextPart(TextFormat.Html) { Text = html };

            // 6) Choose the right security option
            var socketOption = useSsl
                ? SecureSocketOptions.SslOnConnect    // e.g. port 465
                : startTls
                    ? SecureSocketOptions.StartTls    // e.g. port 587
                    : SecureSocketOptions.None;

            // 7) Send the email
            //using var client = new SmtpClient();
            //try
            //{
            //    await client.ConnectAsync(host, port, socketOption);
            //    await client.AuthenticateAsync(user, pass);
            //    await client.SendAsync(message);
            //    await client.DisconnectAsync(true);
            //}
            //catch (Exception ex)
            //{
            //    // Wrap for clarity
            //    throw new InvalidOperationException("Failed to send email. See inner exception for details.", ex);
            //}

            // Inside your SendAsync method, before ConnectAsync
            using var client = new SmtpClient();

            // DEV ONLY: Accept all certificates — REMOVE in production!
            client.ServerCertificateValidationCallback = (s, c, h, e) => true;

            try
            {
                await client.ConnectAsync(host, port, socketOption);
                await client.AuthenticateAsync(user, pass);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to send email. See inner exception for details.", ex);
            }
        }
    }
}
