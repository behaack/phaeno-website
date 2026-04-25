using phaeno.api.Infrastructure.NotificationServices.Email;

namespace phaeno.api.Infrastructure.NotificationServices
{
    public class NotificationService(IEmailNotifier email) : INotificationService
    {
        public async Task NewMailingListContactAsync(
            string userEmail,
            string firstName,
            string lastName,
            string organizationName,
            bool sendBrochure)
        {
            await email.SendNewMailingListContactAsync(
                userEmail,
                firstName,
                lastName,
                organizationName,
                sendBrochure);
        }

        public async Task EmailWebTechnicalBriefRequestAsync(
            string userEmail,
            string firstName,
            string lastName)
        {
            await email.FulfillWebTechnicalBriefRequestAsync(
                userEmail,
                firstName,
                lastName);
        }

        public async Task NewWebOrderAsync(
            string userEmail,
            string firstName,
            string lastName,
            string organizationName,
            string description)
        {
            await email.SendNewWebOrderAsync(
                userEmail,
                firstName,
                lastName,
                organizationName,
                description);
        }
    }
}
