namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgPlan;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Reorg.ReorgPlan;

/// <summary>Checks of reorg plan code, run by the project's runner.</summary>
public static class AdaptiveReorgBudgetReportChecks
{
    public static void AdaptiveReorgBudgetReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701036780);
        var items = new List<AdaptiveReorgBudget>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(AdaptiveReorgBudget.NewAdaptiveReorgBudget($"adaptive-reorg-budget-{index}", "sample"));
        }
        var report = AdaptiveReorgBudgetReport.BuildAdaptiveReorgBudgetReport("test", items, clock);
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
        AdaptiveReorgBudgetReportCountsEachRecordOnce();
    }
}
