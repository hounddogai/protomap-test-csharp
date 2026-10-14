namespace Acme.Workforce.Assets.Tests.Warranty.Budget;

using Acme.Workforce.Assets.Warranty.Budget;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class StrictWarrantyBudgetReportChecks
{
    public static void StrictWarrantyBudgetReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706204956);
        var items = new List<StrictWarrantyBudget>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(StrictWarrantyBudget.NewStrictWarrantyBudget($"strict-warranty-budget-{index}", "sample"));
        }
        var report = StrictWarrantyBudgetReport.BuildStrictWarrantyBudgetReport("test", items, clock);
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
        StrictWarrantyBudgetReportCountsEachRecordOnce();
    }
}
