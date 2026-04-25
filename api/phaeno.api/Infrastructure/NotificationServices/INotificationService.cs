namespace phaeno.api.Infrastructure.NotificationServices
{
    public interface INotificationService
    {
        Task NewMailingListContactAsync(string userEmail, string firstName, string lastName, string organizationName, bool sendBrochure);
        Task EmailWebTechnicalBriefRequestAsync(string userEmail, string firstName, string lastName);
        Task NewWebOrderAsync(string userEmail, string firstName, string lastName, string organizationName, string description);
    }
}
