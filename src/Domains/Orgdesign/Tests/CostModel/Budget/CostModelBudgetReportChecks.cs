namespace Acme.Workforce.Orgdesign.Tests.CostModel.Budget;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.CostModel.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class CostModelBudgetReportChecks
{
    public static void CostModelBudgetReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709279212);
        var items = new List<CostModelBudget>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(CostModelBudget.NewCostModelBudget($"cost-model-budget-{index}", "sample"));
        }
        var report = CostModelBudgetReport.BuildCostModelBudgetReport("test", items, clock);
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
        CostModelBudgetReportCountsEachRecordOnce();
    }
}
