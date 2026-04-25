using Microsoft.EntityFrameworkCore;
using phaeno.api.Features.WebOps.Entities;

namespace phaeno.api.Infrastructure.Db;

public class PseqDatabase(DbContextOptions<PseqDatabase> options) : DbContext(options)
{
    public required DbSet<WebContact> WebContacts { get; set; }
    public required DbSet<WebOrder> WebOrders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WebContact>()
            .Property(x => x.CreatedAtUtc)
            .HasDefaultValueSql("timezone('utc', now())");
    }
}


