namespace Acme.Workforce.Absence.Tests.Balance.BalanceTarget;

using Acme.Workforce.Absence.Balance.BalanceTarget;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of balance target code, run by the project's runner.</summary>
public static class RegionalBalanceTargetReportChecks
{
    public static void RegionalBalanceTargetReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702773095);
        var items = new List<RegionalBalanceTarget>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(RegionalBalanceTarget.NewRegionalBalanceTarget($"regional-balance-target-{index}", "sample"));
        }
        var report = RegionalBalanceTargetReport.BuildRegionalBalanceTargetReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 2, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        RegionalBalanceTargetReportCountsEachRecordOnce();
    }
}
