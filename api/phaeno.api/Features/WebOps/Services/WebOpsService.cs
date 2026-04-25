using phaeno.api.Common.Exceptions.Conflict;
using phaeno.api.Features.WebOps.DataAccess;
using phaeno.api.Features.WebOps.DTOs;
using phaeno.api.Infrastructure.NotificationServices;
using phaeno.api.Infrastructure.RecatchaServices;

namespace phaeno.api.Features.WebOps.Services
{
    public class WebOpsService(WebOpsCommands commands, WebOpsQueries queries, RecaptchaService recaptchaService, INotificationService notificationService)
    {
        public async Task CreateOrder(WebOrderRequestDto dto, CancellationToken ct = default)
        {
            var isValid = await recaptchaService.ValidReCaptchaCodeAsync(dto.RecaptchaCode, dto.RecaptchaAction, ct);

            if (!isValid)
                throw new BadRequestException("Recaptcha not valid");

            var order = dto.WebOrder;
            order.Id = Guid.NewGuid();
            order.Email = order.Email.ToUpper().Normalize();
            await commands.AddWebOrderAsync(order, ct);
            await notificationService.NewWebOrderAsync(order.Email, order.FirstName, order.LastName, order.OrganizationName, order.Description);
        }

        public async Task CreateContact(WebContactRequestDto dto, CancellationToken ct = default)
        {
            var isValid = await recaptchaService.ValidReCaptchaCodeAsync(dto.RecaptchaCode, dto.RecaptchaAction, ct);

            if (!isValid)
                throw new BadRequestException("Recaptcha not valid");

            var existingContact = await queries.GetByEmailAsync(dto.WebContact.Email, ct);
            if (existingContact != null)
                throw new EmailAlreadyInUseException();

            var contact = dto.WebContact;
            contact.Id = Guid.NewGuid();
            contact.NormalizedEmail = contact.Email.ToUpper().Normalize();
            contact.Email = contact.Email.ToLower();
            await commands.AddWebContactAsync(contact, ct);
            bool sendBrochure = contact.SendBrochure.HasValue ? contact.SendBrochure.Value : false;
            await notificationService.NewMailingListContactAsync(contact.Email, contact.FirstName, contact.LastName, contact.OrganizationName, sendBrochure);
            if (sendBrochure)
                await notificationService.EmailWebTechnicalBriefRequestAsync(contact.Email, contact.FirstName, contact.LastName);
        }
    }
}
