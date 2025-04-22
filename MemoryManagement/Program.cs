using System;
using System.IO;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;

namespace EmailNotificationApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var smtpSettings = config.GetSection("SmtpSettings").Get<SmtpSettings>();

            Console.WriteLine("Enter your email address:");
            string? recipientEmail = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(recipientEmail))
            {
                Console.WriteLine("Email address cannot be empty.");
                return;
            }

            if (!IsValidEmail(recipientEmail))
            {
                Console.WriteLine("Invalid email address format.");
                return;
            }

            try
            {
                await SendThankYouEmailAsync(recipientEmail, smtpSettings!);
                Console.WriteLine("Thank you email sent successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send email: {ex.Message}");
            }
        }

        static async Task SendThankYouEmailAsync(string recipientEmail, SmtpSettings smtpSettings)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(smtpSettings.FromName, smtpSettings.FromEmail));
            message.To.Add(MailboxAddress.Parse(recipientEmail));
            message.Subject = "Thank You for Subscribing!";
            message.Body = new TextPart("plain")
            {
                Text = "Hello,\n\nThank you for subscribing to our newsletter. We're glad to have you!\n\nBest regards,\nThe Team"
            };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(smtpSettings.Host, smtpSettings.Port, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(smtpSettings.Username, smtpSettings.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }

        }

        static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
    public class SmtpSettings
    {
        public string? Host { get; set; }
        public int Port { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? FromEmail { get; set; }
        public string? FromName { get; set; }
    }

}

