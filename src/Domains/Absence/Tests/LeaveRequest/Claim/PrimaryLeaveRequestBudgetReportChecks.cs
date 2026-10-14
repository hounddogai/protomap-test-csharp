namespace Acme.Workforce.Absence.Tests.LeaveRequest.Claim;

using Acme.Workforce.Absence.LeaveRequest.Claim;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class PrimaryLeaveRequestBudgetReportChecks
{
    public static void PrimaryLeaveRequestBudgetReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707077497);
        var items = new List<PrimaryLeaveRequestBudget>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(PrimaryLeaveRequestBudget.NewPrimaryLeaveRequestBudget($"primary-leave-request-budget-{index}", "sample"));
        }
        var report = PrimaryLeaveRequestBudgetReport.BuildPrimaryLeaveRequestBudgetReport("test", items, clock);
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
        PrimaryLeaveRequestBudgetReportCountsEachRecordOnce();
    }
}
