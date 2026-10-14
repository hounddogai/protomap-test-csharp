namespace Acme.Workforce.Absence.Tests.ReturnPlan.Ledger;

using Acme.Workforce.Absence.ReturnPlan.Ledger;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class ReturnPlanWindowReportChecks
{
    public static void ReturnPlanWindowReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709274072);
        var items = new List<ReturnPlanWindow>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(ReturnPlanWindow.NewReturnPlanWindow($"return-plan-window-{index}", "sample"));
        }
        var report = ReturnPlanWindowReport.BuildReturnPlanWindowReport("test", items, clock);
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
        ReturnPlanWindowReportCountsEachRecordOnce();
    }
}
