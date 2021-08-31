using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Threading.Tasks;

namespace SkisporetUpdates.Helpers
{
    public class Mail
    {
        public static async Task<Response> SendTrackUpdateMail()
        {
            var apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY");
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("knaben.loypekjoring@gmail.com", "Knaben Loypekjoring");
            var to = new EmailAddress("ma.tyldum@gmail.com", "Mats");
            var subject = "Skisporet oppdatering!";
            var plainTextContent = "Woho! Løypemaskinen er i gang. Sjekk status på skisporet.no";
            var htmlContent = "<p>Woho! Løypemaskinen er i gang. Sjekk status på <a href=\"https://skisporet.no/setView/58.6657916/7.0643317/15/norges_grunnkart\">skisporet.no</a></p>";
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);
            return response;
        }
    }
}
