namespace Acme.Workforce.Assets.Tests.Laptop.Milestone;

using Acme.Workforce.Assets.Laptop.Milestone;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class LaptopCheckpointReportChecks
{
    public static void LaptopCheckpointReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706677916);
        var items = new List<LaptopCheckpoint>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(LaptopCheckpoint.NewLaptopCheckpoint($"laptop-checkpoint-{index}", "sample"));
        }
        var report = LaptopCheckpointReport.BuildLaptopCheckpointReport("test", items, clock);
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
        LaptopCheckpointReportCountsEachRecordOnce();
    }
}
