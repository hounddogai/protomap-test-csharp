namespace Acme.Workforce.Compensation.Tests.MeritCycle.Budget;

using Acme.Workforce.Compensation.MeritCycle.Budget;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class CappedMeritCycleBudgetReportChecks
{
    public static void CappedMeritCycleBudgetReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709930240);
        var items = new List<CappedMeritCycleBudget>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(CappedMeritCycleBudget.NewCappedMeritCycleBudget($"capped-merit-cycle-budget-{index}", "sample"));
        }
        var report = CappedMeritCycleBudgetReport.BuildCappedMeritCycleBudgetReport("test", items, clock);
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
        CappedMeritCycleBudgetReportCountsEachRecordOnce();
    }
}
