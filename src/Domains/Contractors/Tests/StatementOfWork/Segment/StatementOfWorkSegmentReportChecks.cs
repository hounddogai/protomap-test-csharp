namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Segment;

using Acme.Workforce.Contractors.StatementOfWork.Segment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class StatementOfWorkSegmentReportChecks
{
    public static void StatementOfWorkSegmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706809740);
        var items = new List<StatementOfWorkSegment>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(StatementOfWorkSegment.NewStatementOfWorkSegment($"statement-of-work-segment-{index}", "sample"));
        }
        var report = StatementOfWorkSegmentReport.BuildStatementOfWorkSegmentReport("test", items, clock);
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
        StatementOfWorkSegmentReportCountsEachRecordOnce();
    }
}
