namespace Acme.Workforce.Workplace.Tests.Visitor.RecordItem;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Visitor.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class VisitorSnapshotReportChecks
{
    public static void VisitorSnapshotReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706754078);
        var items = new List<VisitorSnapshot>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(VisitorSnapshot.NewVisitorSnapshot($"visitor-snapshot-{index}", "sample"));
        }
        var report = VisitorSnapshotReport.BuildVisitorSnapshotReport("test", items, clock);
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
        VisitorSnapshotReportCountsEachRecordOnce();
    }
}
