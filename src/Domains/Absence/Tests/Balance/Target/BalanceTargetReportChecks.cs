namespace Acme.Workforce.Absence.Tests.Balance.Target;

using Acme.Workforce.Absence.Balance.Target;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class BalanceTargetReportChecks
{
    public static void BalanceTargetReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704041293);
        var items = new List<BalanceTarget>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(BalanceTarget.NewBalanceTarget($"balance-target-{index}", "sample"));
        }
        var report = BalanceTargetReport.BuildBalanceTargetReport("test", items, clock);
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
        BalanceTargetReportCountsEachRecordOnce();
    }
}
