namespace Acme.Workforce.Talent.Tests.SuccessionPlan.PlanAgreement;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.SuccessionPlan.PlanAgreement;

/// <summary>Checks of plan agreement code, run by the project's runner.</summary>
public static class SuccessionPlanAllocationReportChecks
{
    public static void SuccessionPlanAllocationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708029370);
        var items = new List<SuccessionPlanAllocation>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(SuccessionPlanAllocation.NewSuccessionPlanAllocation($"succession-plan-allocation-{index}", "sample"));
        }
        var report = SuccessionPlanAllocationReport.BuildSuccessionPlanAllocationReport("test", items, clock);
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
        SuccessionPlanAllocationReportCountsEachRecordOnce();
    }
}
