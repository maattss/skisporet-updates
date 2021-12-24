using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Threading.Tasks;

namespace SkisporetUpdates.Helpers
{
    public static class Mail
    {
        public static async Task<Response> SendTrackUpdateMail()
        {
            var apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY");
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("knaben.loypekjoring@gmail.com", "Knaben Loypekjoring");
            var to = new EmailAddress("ma.tyldum@gmail.com", "Mats");
            const string subject = "Skisporet oppdatering!";
            const string plainTextContent = "Yes! Løypemaskinen er i gang. Sjekk status på skisporet.no";
            const string htmlContent = "<p>Yes! Løypemaskinen er i gang. Sjekk status på <a href=\"https://skisporet.no/setView/58.6657916/7.0643317/15/norges_grunnkart\">skisporet.no</a></p>";
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            return await client.SendEmailAsync(msg);
        }
    }
}
