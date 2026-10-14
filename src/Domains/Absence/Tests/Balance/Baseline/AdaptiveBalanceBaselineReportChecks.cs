namespace Acme.Workforce.Absence.Tests.Balance.Baseline;

using Acme.Workforce.Absence.Balance.Baseline;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class AdaptiveBalanceBaselineReportChecks
{
    public static void AdaptiveBalanceBaselineReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709328420);
        var items = new List<AdaptiveBalanceBaseline>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(AdaptiveBalanceBaseline.NewAdaptiveBalanceBaseline($"adaptive-balance-baseline-{index}", "sample"));
        }
        var report = AdaptiveBalanceBaselineReport.BuildAdaptiveBalanceBaselineReport("test", items, clock);
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
        AdaptiveBalanceBaselineReportCountsEachRecordOnce();
    }
}
