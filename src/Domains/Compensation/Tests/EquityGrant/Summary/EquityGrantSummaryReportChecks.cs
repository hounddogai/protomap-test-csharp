namespace Acme.Workforce.Compensation.Tests.EquityGrant.Summary;

using Acme.Workforce.Compensation.EquityGrant.Summary;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class EquityGrantSummaryReportChecks
{
    public static void EquityGrantSummaryReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702247396);
        var items = new List<EquityGrantSummary>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(EquityGrantSummary.NewEquityGrantSummary($"equity-grant-summary-{index}", "sample"));
        }
        var report = EquityGrantSummaryReport.BuildEquityGrantSummaryReport("test", items, clock);
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
        EquityGrantSummaryReportCountsEachRecordOnce();
    }
}
