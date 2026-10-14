namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Agreement;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.SuccessionPlan.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class SuccessionPlanCycleReportChecks
{
    public static void SuccessionPlanCycleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707170155);
        var items = new List<SuccessionPlanCycle>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(SuccessionPlanCycle.NewSuccessionPlanCycle($"succession-plan-cycle-{index}", "sample"));
        }
        var report = SuccessionPlanCycleReport.BuildSuccessionPlanCycleReport("test", items, clock);
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
        SuccessionPlanCycleReportCountsEachRecordOnce();
    }
}
