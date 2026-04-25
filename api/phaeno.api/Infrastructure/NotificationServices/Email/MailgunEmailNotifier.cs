using Microsoft.Extensions.Options;
using phaeno.api.Configuration;

namespace phaeno.api.Infrastructure.NotificationServices.Email
{
    public class MailgunEmailNotifier : IEmailNotifier
    {
        private readonly HttpClient http;
        private readonly EmailServiceSettings settings;

        public MailgunEmailNotifier(
            HttpClient http,
            IOptions<EmailServiceSettings> settings)
        {
            this.http = http;
            this.settings = settings.Value;
        }

        public async Task SendNewMailingListContactAsync(
            string userEmail,
            string firstName,
            string lastName,
            string organizationName,
            bool sendBrochure)
        {
            await SendAsync(new MailgunTemplateMessage(
                Template: "new-mailing-list-contact",
                To: EmailAddress(settings.PhaenoAccountName, settings.AccountTo),
                Variables: new
                {
                    firstName,
                    lastName,
                    organizationName,
                    email = userEmail,
                    sendBrochure
                }));
        }

        public async Task FulfillWebTechnicalBriefRequestAsync(
            string userEmail,
            string firstName,
            string lastName)
        {
            const string technicalBriefPath =
                "https://webops.phaenobiotech.com/public/technical-brief-C660184C-47D0-45AA-872F-8B3538F17BE5/PSeq-Technical-Brief.AD6548E7-F66A-429A-B0F6-A63988935D68.pdf";

            await SendAsync(new MailgunTemplateMessage(
                Template: "fulfill-web-technical-brief-request",
                To: EmailAddress(firstName, lastName, userEmail),
                Variables: new
                {
                    firstName,
                    lastName,
                    technicalBriefPath
                }));
        }

        public async Task SendNewWebOrderAsync(
            string userEmail,
            string firstName,
            string lastName,
            string organizationName,
            string description)
        {
            await SendAsync(new MailgunTemplateMessage(
                Template: "new-web-order",
                To: EmailAddress(settings.PhaenoAccountName, settings.AccountTo),
                Variables: new
                {
                    firstName,
                    lastName,
                    organizationName,
                    email = userEmail,
                    description
                }));
        }

        private async Task SendAsync(MailgunTemplateMessage message)
        {
            var form = new Dictionary<string, string>
            {
                ["from"] = settings.AccountFrom,
                ["to"] = message.To,
                ["template"] = message.Template,
                ["o:tracking"] = "false",
                ["o:tracking-clicks"] = "no",
                ["o:require-tls"] = "true",
                ["o:skip-verification"] = "true",
                ["o:dkim"] = "yes",
                ["o:tag"] = "transactional"
            };

            foreach (var (key, value) in message.Variables
                .GetType()
                .GetProperties()
                .Select(p => (p.Name, p.GetValue(message.Variables)?.ToString())))
            {
                if (value != null)
                    form[$"v:{key}"] = value;
            }

            try
            {
                var url = string.Format("{0}/{1}", settings.Url, settings.Resource);
                var response = await http.PostAsync(url, new FormUrlEncodedContent(form));
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static string EmailAddress(string firstName, string lastName, string email)
        {
            var name = string.Format("{0} {1}", firstName, lastName);
            return EmailAddress(name, email);
        }

        private static string EmailAddress(string name, string email)
        {
            return string.Format("{0} <{1}>", name, email);
        }
    }
}
