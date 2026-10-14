namespace Acme.Workforce.Workplace.Tests.Visitor.Review;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Visitor.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class VisitorNoticeReportChecks
{
    public static void VisitorNoticeReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704026425);
        var items = new List<VisitorNotice>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(VisitorNotice.NewVisitorNotice($"visitor-notice-{index}", "sample"));
        }
        var report = VisitorNoticeReport.BuildVisitorNoticeReport("test", items, clock);
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
        VisitorNoticeReportCountsEachRecordOnce();
    }
}
