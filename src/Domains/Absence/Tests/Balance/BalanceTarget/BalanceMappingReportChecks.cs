namespace Acme.Workforce.Absence.Tests.Balance.BalanceTarget;

using Acme.Workforce.Absence.Balance.BalanceTarget;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of balance target code, run by the project's runner.</summary>
public static class BalanceMappingReportChecks
{
    public static void BalanceMappingReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702344876);
        var items = new List<BalanceMapping>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(BalanceMapping.NewBalanceMapping($"balance-mapping-{index}", "sample"));
        }
        var report = BalanceMappingReport.BuildBalanceMappingReport("test", items, clock);
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
        BalanceMappingReportCountsEachRecordOnce();
    }
}
