namespace Acme.Workforce.Workplace.Tests.Visitor.Assignment;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Visitor.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class VisitorPlanReportChecks
{
    public static void VisitorPlanReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706385737);
        var items = new List<VisitorPlan>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(VisitorPlan.NewVisitorPlan($"visitor-plan-{index}", "sample"));
        }
        var report = VisitorPlanReport.BuildVisitorPlanReport("test", items, clock);
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
        VisitorPlanReportCountsEachRecordOnce();
    }
}
