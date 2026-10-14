namespace Acme.Workforce.Absence.Tests.Holiday.OverrideItem;

using Acme.Workforce.Absence.Holiday.OverrideItem;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class HolidayBudgetReportChecks
{
    public static void HolidayBudgetReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706052429);
        var items = new List<HolidayBudget>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(HolidayBudget.NewHolidayBudget($"holiday-budget-{index}", "sample"));
        }
        var report = HolidayBudgetReport.BuildHolidayBudgetReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 2, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        HolidayBudgetReportCountsEachRecordOnce();
    }
}
