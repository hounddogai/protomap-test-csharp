namespace Acme.Workforce.Absence.Tests.Balance.Threshold;

using Acme.Workforce.Absence.Balance.Threshold;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class PrimaryBalanceMetricReportChecks
{
    public static void PrimaryBalanceMetricReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707505536);
        var items = new List<PrimaryBalanceMetric>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(PrimaryBalanceMetric.NewPrimaryBalanceMetric($"primary-balance-metric-{index}", "sample"));
        }
        var report = PrimaryBalanceMetricReport.BuildPrimaryBalanceMetricReport("test", items, clock);
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
        PrimaryBalanceMetricReportCountsEachRecordOnce();
    }
}
