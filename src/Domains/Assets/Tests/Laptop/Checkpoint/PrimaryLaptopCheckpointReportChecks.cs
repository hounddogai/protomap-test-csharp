namespace Acme.Workforce.Assets.Tests.Laptop.Checkpoint;

using Acme.Workforce.Assets.Laptop.Checkpoint;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class PrimaryLaptopCheckpointReportChecks
{
    public static void PrimaryLaptopCheckpointReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706866341);
        var items = new List<PrimaryLaptopCheckpoint>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(PrimaryLaptopCheckpoint.NewPrimaryLaptopCheckpoint($"primary-laptop-checkpoint-{index}", "sample"));
        }
        var report = PrimaryLaptopCheckpointReport.BuildPrimaryLaptopCheckpointReport("test", items, clock);
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
        PrimaryLaptopCheckpointReportCountsEachRecordOnce();
    }
}
