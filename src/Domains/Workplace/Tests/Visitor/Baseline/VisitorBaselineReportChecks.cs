namespace Acme.Workforce.Workplace.Tests.Visitor.Baseline;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Visitor.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class VisitorBaselineReportChecks
{
    public static void VisitorBaselineReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702798728);
        var items = new List<VisitorBaseline>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(VisitorBaseline.NewVisitorBaseline($"visitor-baseline-{index}", "sample"));
        }
        var report = VisitorBaselineReport.BuildVisitorBaselineReport("test", items, clock);
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
        VisitorBaselineReportCountsEachRecordOnce();
    }
}
