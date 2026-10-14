namespace Acme.Workforce.Absence.Tests.Balance.Rollup;

using Acme.Workforce.Absence.Balance.Rollup;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class FallbackBalanceAdjustmentReportChecks
{
    public static void FallbackBalanceAdjustmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707220906);
        var items = new List<FallbackBalanceAdjustment>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(FallbackBalanceAdjustment.NewFallbackBalanceAdjustment($"fallback-balance-adjustment-{index}", "sample"));
        }
        var report = FallbackBalanceAdjustmentReport.BuildFallbackBalanceAdjustmentReport("test", items, clock);
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
        FallbackBalanceAdjustmentReportCountsEachRecordOnce();
    }
}
