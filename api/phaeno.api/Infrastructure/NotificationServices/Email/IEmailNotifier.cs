namespace phaeno.api.Infrastructure.NotificationServices.Email
{
    public interface IEmailNotifier
    {
        Task SendNewMailingListContactAsync(string userEmail, string firstName, string lastName, string organizationName, bool sendBrochure);
        Task FulfillWebTechnicalBriefRequestAsync(string userEmail, string firstName, string lastName);
        Task SendNewWebOrderAsync(string userEmail, string firstName, string lastName, string organizationName, string description);
    }
}
