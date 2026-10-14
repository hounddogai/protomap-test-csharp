namespace Acme.Workforce.Workplace.Tests.Desk.Budget;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Desk.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class StrictDeskBudgetReportChecks
{
    public static void StrictDeskBudgetReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702180608);
        var items = new List<StrictDeskBudget>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(StrictDeskBudget.NewStrictDeskBudget($"strict-desk-budget-{index}", "sample"));
        }
        var report = StrictDeskBudgetReport.BuildStrictDeskBudgetReport("test", items, clock);
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
        StrictDeskBudgetReportCountsEachRecordOnce();
    }
}
