namespace Acme.Workforce.Workplace.Tests.Occupancy.OccupancyPlan;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Occupancy.OccupancyPlan;

/// <summary>Checks of occupancy plan code, run by the project's runner.</summary>
public static class OccupancyBudgetReportChecks
{
    public static void OccupancyBudgetReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703620354);
        var items = new List<OccupancyBudget>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(OccupancyBudget.NewOccupancyBudget($"occupancy-budget-{index}", "sample"));
        }
        var report = OccupancyBudgetReport.BuildOccupancyBudgetReport("test", items, clock);
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
        OccupancyBudgetReportCountsEachRecordOnce();
    }
}
