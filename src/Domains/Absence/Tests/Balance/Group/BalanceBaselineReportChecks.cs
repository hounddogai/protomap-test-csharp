namespace Acme.Workforce.Absence.Tests.Balance.Group;

using Acme.Workforce.Absence.Balance.Group;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class BalanceBaselineReportChecks
{
    public static void BalanceBaselineReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708193529);
        var items = new List<BalanceBaseline>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(BalanceBaseline.NewBalanceBaseline($"balance-baseline-{index}", "sample"));
        }
        var report = BalanceBaselineReport.BuildBalanceBaselineReport("test", items, clock);
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
        BalanceBaselineReportCountsEachRecordOnce();
    }
}
