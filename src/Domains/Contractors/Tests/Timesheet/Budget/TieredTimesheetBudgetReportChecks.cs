namespace Acme.Workforce.Contractors.Tests.Timesheet.Budget;

using Acme.Workforce.Contractors.Timesheet.Budget;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class TieredTimesheetBudgetReportChecks
{
    public static void TieredTimesheetBudgetReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704282363);
        var items = new List<TieredTimesheetBudget>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(TieredTimesheetBudget.NewTieredTimesheetBudget($"tiered-timesheet-budget-{index}", "sample"));
        }
        var report = TieredTimesheetBudgetReport.BuildTieredTimesheetBudgetReport("test", items, clock);
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
        TieredTimesheetBudgetReportCountsEachRecordOnce();
    }
}
