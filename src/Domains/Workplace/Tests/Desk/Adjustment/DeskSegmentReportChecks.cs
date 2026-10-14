namespace Acme.Workforce.Workplace.Tests.Desk.Adjustment;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Desk.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class DeskSegmentReportChecks
{
    public static void DeskSegmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706466302);
        var items = new List<DeskSegment>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(DeskSegment.NewDeskSegment($"desk-segment-{index}", "sample"));
        }
        var report = DeskSegmentReport.BuildDeskSegmentReport("test", items, clock);
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
        DeskSegmentReportCountsEachRecordOnce();
    }
}
