namespace Acme.Workforce.Talent.Tests.Mobility.Window;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Mobility.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class MobilityCheckpointReportChecks
{
    public static void MobilityCheckpointReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700938544);
        var items = new List<MobilityCheckpoint>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(MobilityCheckpoint.NewMobilityCheckpoint($"mobility-checkpoint-{index}", "sample"));
        }
        var report = MobilityCheckpointReport.BuildMobilityCheckpointReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 4, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        MobilityCheckpointReportCountsEachRecordOnce();
    }
}
