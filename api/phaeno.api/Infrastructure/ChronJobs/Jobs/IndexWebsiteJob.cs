using phaeno.api.Infrastructure.WebcrawlerServices;
using Quartz;

namespace phaeno.api.Infrastructure.ChronJobs.Jobs;

public sealed class IndexWebsiteJob : IJob
{
    private readonly IWebcrawlerService webcrawlerService;
    private readonly ILogger<IndexWebsiteJob> logger;

    public IndexWebsiteJob(
        IWebcrawlerService webcrawlerService,
        ILogger<IndexWebsiteJob> logger)
    {
        this.webcrawlerService = webcrawlerService;
        this.logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            logger.LogInformation("[Quartz] Website crawl started.");

            await webcrawlerService
                .CrawlPhaenoSiteAsync(context.CancellationToken);

            stopwatch.Stop();

            logger.LogInformation(
                "[Quartz] Website crawl completed in {ElapsedMs} ms.",
                stopwatch.ElapsedMilliseconds);
        }
        catch (OperationCanceledException)
        {
            stopwatch.Stop();

            logger.LogWarning(
                "[Quartz] Website crawl cancelled after {ElapsedMs} ms.",
                stopwatch.ElapsedMilliseconds);

            // Important: propagate cancellation to Quartz
            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            logger.LogError(
                ex,
                "[Quartz] Website crawl failed after {ElapsedMs} ms.",
                stopwatch.ElapsedMilliseconds);

            // IMPORTANT: rethrow so Quartz marks job as failed
            throw;
        }
    }
}
