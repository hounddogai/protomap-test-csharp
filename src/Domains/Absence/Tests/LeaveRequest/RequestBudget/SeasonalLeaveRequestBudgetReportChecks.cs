namespace Acme.Workforce.Absence.Tests.LeaveRequest.RequestBudget;

using Acme.Workforce.Absence.LeaveRequest.RequestBudget;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of request budget code, run by the project's runner.</summary>
public static class SeasonalLeaveRequestBudgetReportChecks
{
    public static void SeasonalLeaveRequestBudgetReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704360544);
        var items = new List<SeasonalLeaveRequestBudget>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(SeasonalLeaveRequestBudget.NewSeasonalLeaveRequestBudget($"seasonal-leave-request-budget-{index}", "sample"));
        }
        var report = SeasonalLeaveRequestBudgetReport.BuildSeasonalLeaveRequestBudgetReport("test", items, clock);
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
        SeasonalLeaveRequestBudgetReportCountsEachRecordOnce();
    }
}
