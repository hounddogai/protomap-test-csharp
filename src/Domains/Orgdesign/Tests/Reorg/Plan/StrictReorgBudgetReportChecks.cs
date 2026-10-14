namespace Acme.Workforce.Orgdesign.Tests.Reorg.Plan;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Reorg.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class StrictReorgBudgetReportChecks
{
    public static void StrictReorgBudgetReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704506551);
        var items = new List<StrictReorgBudget>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(StrictReorgBudget.NewStrictReorgBudget($"strict-reorg-budget-{index}", "sample"));
        }
        var report = StrictReorgBudgetReport.BuildStrictReorgBudgetReport("test", items, clock);
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
        StrictReorgBudgetReportCountsEachRecordOnce();
    }
}
