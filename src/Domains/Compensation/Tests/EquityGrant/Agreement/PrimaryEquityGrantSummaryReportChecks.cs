namespace Acme.Workforce.Compensation.Tests.EquityGrant.Agreement;

using Acme.Workforce.Compensation.EquityGrant.Agreement;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class PrimaryEquityGrantSummaryReportChecks
{
    public static void PrimaryEquityGrantSummaryReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705313263);
        var items = new List<PrimaryEquityGrantSummary>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(PrimaryEquityGrantSummary.NewPrimaryEquityGrantSummary($"primary-equity-grant-summary-{index}", "sample"));
        }
        var report = PrimaryEquityGrantSummaryReport.BuildPrimaryEquityGrantSummaryReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 6, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        PrimaryEquityGrantSummaryReportCountsEachRecordOnce();
    }
}
