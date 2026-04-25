namespace phaeno.api.Configuration;

public class ChronJobScheduleOptions
{
    public JobSchedule IndexWebsite { get; init; } = new();

    public sealed class JobSchedule
    {
        public int IntervalHours { get; init; }
        public bool RunOnStartup { get; init; }
    }
}
