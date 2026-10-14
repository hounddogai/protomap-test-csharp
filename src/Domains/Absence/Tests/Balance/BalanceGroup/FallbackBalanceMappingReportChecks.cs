namespace Acme.Workforce.Absence.Tests.Balance.BalanceGroup;

using Acme.Workforce.Absence.Balance.BalanceGroup;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of balance group code, run by the project's runner.</summary>
public static class FallbackBalanceMappingReportChecks
{
    public static void FallbackBalanceMappingReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708154816);
        var items = new List<FallbackBalanceMapping>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(FallbackBalanceMapping.NewFallbackBalanceMapping($"fallback-balance-mapping-{index}", "sample"));
        }
        var report = FallbackBalanceMappingReport.BuildFallbackBalanceMappingReport("test", items, clock);
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
        FallbackBalanceMappingReportCountsEachRecordOnce();
    }
}
