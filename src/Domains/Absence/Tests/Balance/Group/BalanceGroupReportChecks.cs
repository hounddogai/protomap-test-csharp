namespace Acme.Workforce.Absence.Tests.Balance.Group;

using Acme.Workforce.Absence.Balance.Group;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class BalanceGroupReportChecks
{
    public static void BalanceGroupReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707847353);
        var items = new List<BalanceGroup>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(BalanceGroup.NewBalanceGroup($"balance-group-{index}", "sample"));
        }
        var report = BalanceGroupReport.BuildBalanceGroupReport("test", items, clock);
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
        BalanceGroupReportCountsEachRecordOnce();
    }
}
