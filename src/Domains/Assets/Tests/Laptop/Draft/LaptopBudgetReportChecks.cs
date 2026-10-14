namespace Acme.Workforce.Assets.Tests.Laptop.Draft;

using Acme.Workforce.Assets.Laptop.Draft;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class LaptopBudgetReportChecks
{
    public static void LaptopBudgetReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707751534);
        var items = new List<LaptopBudget>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(LaptopBudget.NewLaptopBudget($"laptop-budget-{index}", "sample"));
        }
        var report = LaptopBudgetReport.BuildLaptopBudgetReport("test", items, clock);
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
        LaptopBudgetReportCountsEachRecordOnce();
    }
}
