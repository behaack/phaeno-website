namespace phaeno.api.Infrastructure.WebcrawlerServices
{
    public interface IWebcrawlerService
    {
        Task CrawlPhaenoSiteAsync(CancellationToken ct = default);
    }
}
