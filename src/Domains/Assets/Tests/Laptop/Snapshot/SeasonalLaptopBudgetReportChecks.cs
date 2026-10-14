namespace Acme.Workforce.Assets.Tests.Laptop.Snapshot;

using Acme.Workforce.Assets.Laptop.Snapshot;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class SeasonalLaptopBudgetReportChecks
{
    public static void SeasonalLaptopBudgetReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705470772);
        var items = new List<SeasonalLaptopBudget>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(SeasonalLaptopBudget.NewSeasonalLaptopBudget($"seasonal-laptop-budget-{index}", "sample"));
        }
        var report = SeasonalLaptopBudgetReport.BuildSeasonalLaptopBudgetReport("test", items, clock);
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
        SeasonalLaptopBudgetReportCountsEachRecordOnce();
    }
}
