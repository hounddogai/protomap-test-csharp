namespace Acme.Workforce.Orgdesign.Tests.Headcount.OverrideItem;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Headcount.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class HeadcountSummaryReportChecks
{
    public static void HeadcountSummaryReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709353815);
        var items = new List<HeadcountSummary>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(HeadcountSummary.NewHeadcountSummary($"headcount-summary-{index}", "sample"));
        }
        var report = HeadcountSummaryReport.BuildHeadcountSummaryReport("test", items, clock);
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
        HeadcountSummaryReportCountsEachRecordOnce();
    }
}
