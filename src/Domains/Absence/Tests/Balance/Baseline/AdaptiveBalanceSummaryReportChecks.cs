namespace Acme.Workforce.Absence.Tests.Balance.Baseline;

using Acme.Workforce.Absence.Balance.Baseline;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class AdaptiveBalanceSummaryReportChecks
{
    public static void AdaptiveBalanceSummaryReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701412345);
        var items = new List<AdaptiveBalanceSummary>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(AdaptiveBalanceSummary.NewAdaptiveBalanceSummary($"adaptive-balance-summary-{index}", "sample"));
        }
        var report = AdaptiveBalanceSummaryReport.BuildAdaptiveBalanceSummaryReport("test", items, clock);
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
        AdaptiveBalanceSummaryReportCountsEachRecordOnce();
    }
}
