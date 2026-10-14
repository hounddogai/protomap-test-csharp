namespace Acme.Workforce.Engagement.Tests.TownHall.HallTier;

using Acme.Workforce.Engagement.TownHall.HallTier;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of hall tier code, run by the project's runner.</summary>
public static class TownHallBudgetReportChecks
{
    public static void TownHallBudgetReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709134856);
        var items = new List<TownHallBudget>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(TownHallBudget.NewTownHallBudget($"town-hall-budget-{index}", "sample"));
        }
        var report = TownHallBudgetReport.BuildTownHallBudgetReport("test", items, clock);
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
        TownHallBudgetReportCountsEachRecordOnce();
    }
}
