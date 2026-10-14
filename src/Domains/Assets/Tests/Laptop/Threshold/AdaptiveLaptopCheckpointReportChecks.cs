namespace Acme.Workforce.Assets.Tests.Laptop.Threshold;

using Acme.Workforce.Assets.Laptop.Threshold;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class AdaptiveLaptopCheckpointReportChecks
{
    public static void AdaptiveLaptopCheckpointReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709436617);
        var items = new List<AdaptiveLaptopCheckpoint>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(AdaptiveLaptopCheckpoint.NewAdaptiveLaptopCheckpoint($"adaptive-laptop-checkpoint-{index}", "sample"));
        }
        var report = AdaptiveLaptopCheckpointReport.BuildAdaptiveLaptopCheckpointReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 5, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        AdaptiveLaptopCheckpointReportCountsEachRecordOnce();
    }
}
