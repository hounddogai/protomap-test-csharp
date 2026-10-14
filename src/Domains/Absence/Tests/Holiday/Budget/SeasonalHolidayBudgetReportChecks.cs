namespace Acme.Workforce.Absence.Tests.Holiday.Budget;

using Acme.Workforce.Absence.Holiday.Budget;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class SeasonalHolidayBudgetReportChecks
{
    public static void SeasonalHolidayBudgetReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701630742);
        var items = new List<SeasonalHolidayBudget>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(SeasonalHolidayBudget.NewSeasonalHolidayBudget($"seasonal-holiday-budget-{index}", "sample"));
        }
        var report = SeasonalHolidayBudgetReport.BuildSeasonalHolidayBudgetReport("test", items, clock);
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
        SeasonalHolidayBudgetReportCountsEachRecordOnce();
    }
}
