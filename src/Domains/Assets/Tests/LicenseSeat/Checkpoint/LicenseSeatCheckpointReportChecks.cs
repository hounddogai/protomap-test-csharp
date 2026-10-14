namespace Acme.Workforce.Assets.Tests.LicenseSeat.Checkpoint;

using Acme.Workforce.Assets.LicenseSeat.Checkpoint;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class LicenseSeatCheckpointReportChecks
{
    public static void LicenseSeatCheckpointReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706506709);
        var items = new List<LicenseSeatCheckpoint>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(LicenseSeatCheckpoint.NewLicenseSeatCheckpoint($"license-seat-checkpoint-{index}", "sample"));
        }
        var report = LicenseSeatCheckpointReport.BuildLicenseSeatCheckpointReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 3, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        LicenseSeatCheckpointReportCountsEachRecordOnce();
    }
}
