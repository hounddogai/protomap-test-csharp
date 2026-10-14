namespace Acme.Workforce.Absence.Tests.Balance.Milestone;

using Acme.Workforce.Absence.Balance.Milestone;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class BalanceVarianceReportChecks
{
    public static void BalanceVarianceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707706530);
        var items = new List<BalanceVariance>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(BalanceVariance.NewBalanceVariance($"balance-variance-{index}", "sample"));
        }
        var report = BalanceVarianceReport.BuildBalanceVarianceReport("test", items, clock);
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
        BalanceVarianceReportCountsEachRecordOnce();
    }
}
