namespace Acme.Workforce.Orgdesign.Tests.Headcount.HeadcountRecord;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Headcount.HeadcountRecord;

/// <summary>Checks of headcount record code, run by the project's runner.</summary>
public static class HeadcountSnapshotReportChecks
{
    public static void HeadcountSnapshotReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706504464);
        var items = new List<HeadcountSnapshot>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(HeadcountSnapshot.NewHeadcountSnapshot($"headcount-snapshot-{index}", "sample"));
        }
        var report = HeadcountSnapshotReport.BuildHeadcountSnapshotReport("test", items, clock);
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
        HeadcountSnapshotReportCountsEachRecordOnce();
    }
}
