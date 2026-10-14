namespace Acme.Workforce.Talent.Tests.Nomination.Adjustment;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Nomination.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class GlobalNominationSnapshotReportChecks
{
    public static void GlobalNominationSnapshotReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704739695);
        var items = new List<GlobalNominationSnapshot>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(GlobalNominationSnapshot.NewGlobalNominationSnapshot($"global-nomination-snapshot-{index}", "sample"));
        }
        var report = GlobalNominationSnapshotReport.BuildGlobalNominationSnapshotReport("test", items, clock);
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
        GlobalNominationSnapshotReportCountsEachRecordOnce();
    }
}
