namespace Acme.Workforce.Orgdesign.Tests.Headcount.Agreement;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Headcount.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class HeadcountCheckpointReportChecks
{
    public static void HeadcountCheckpointReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708462594);
        var items = new List<HeadcountCheckpoint>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(HeadcountCheckpoint.NewHeadcountCheckpoint($"headcount-checkpoint-{index}", "sample"));
        }
        var report = HeadcountCheckpointReport.BuildHeadcountCheckpointReport("test", items, clock);
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
        HeadcountCheckpointReportCountsEachRecordOnce();
    }
}
