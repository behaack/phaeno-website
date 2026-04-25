namespace phaeno.api.Infrastructure.NotificationServices.Email
{
    internal sealed record MailgunTemplateMessage(
        string Template,
        string To,
        object Variables
    );
}
