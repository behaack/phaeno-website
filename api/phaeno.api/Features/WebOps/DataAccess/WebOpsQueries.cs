using Microsoft.EntityFrameworkCore;
using phaeno.api.Features.WebOps.Entities;
using phaeno.api.Infrastructure.Db;

namespace phaeno.api.Features.WebOps.DataAccess
{
    public class WebOpsQueries(PseqDatabase db)
    {
        public async Task PingDatabaseAsync(CancellationToken ct = default)
        {
            await db.Database.ExecuteSqlRawAsync("SELECT 1", ct);
        }

        public async Task<WebContact?> GetByEmailAsync(string email, CancellationToken ct = default)
        {
            return await db.WebContacts
                .AsNoTracking()
                .SingleOrDefaultAsync(u =>
                    u.NormalizedEmail == email.ToUpper().Normalize(), ct);
        }
    }
}
