namespace Acme.Workforce.Absence.Tests.Balance.RecordItem;

using Acme.Workforce.Absence.Balance.RecordItem;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class FallbackBalanceThresholdReportChecks
{
    public static void FallbackBalanceThresholdReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701919427);
        var items = new List<FallbackBalanceThreshold>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(FallbackBalanceThreshold.NewFallbackBalanceThreshold($"fallback-balance-threshold-{index}", "sample"));
        }
        var report = FallbackBalanceThresholdReport.BuildFallbackBalanceThresholdReport("test", items, clock);
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
        FallbackBalanceThresholdReportCountsEachRecordOnce();
    }
}
