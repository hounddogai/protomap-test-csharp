namespace Acme.Workforce.Absence.Tests.ReturnPlan.Ledger;

using Acme.Workforce.Absence.ReturnPlan.Ledger;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class ReturnPlanAllocationReportChecks
{
    public static void ReturnPlanAllocationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706313334);
        var items = new List<ReturnPlanAllocation>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(ReturnPlanAllocation.NewReturnPlanAllocation($"return-plan-allocation-{index}", "sample"));
        }
        var report = ReturnPlanAllocationReport.BuildReturnPlanAllocationReport("test", items, clock);
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
        ReturnPlanAllocationReportCountsEachRecordOnce();
    }
}
