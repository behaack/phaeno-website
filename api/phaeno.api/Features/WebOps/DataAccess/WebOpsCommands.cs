using Microsoft.EntityFrameworkCore;
using phaeno.api.Features.WebOps.Entities;
using phaeno.api.Infrastructure.Db;

namespace phaeno.api.Features.WebOps.DataAccess
{
    public class WebOpsCommands(PseqDatabase db)
    {
        public async Task AddWebContactAsync(WebContact webContact, CancellationToken ct = default)
        {
            db.WebContacts.Add(webContact);
            await db.SaveChangesAsync(ct);
        }

        public async Task AddWebOrderAsync(WebOrder webOrder, CancellationToken ct = default)
        {
            db.WebOrders.Add(webOrder);
            await db.SaveChangesAsync(ct);
        }
    }
}
