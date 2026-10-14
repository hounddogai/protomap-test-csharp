namespace Acme.Workforce.Talent.Tests.CareerPath.Plan;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.CareerPath.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class CareerPathPlanReportChecks
{
    public static void CareerPathPlanReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709440043);
        var items = new List<CareerPathPlan>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(CareerPathPlan.NewCareerPathPlan($"career-path-plan-{index}", "sample"));
        }
        var report = CareerPathPlanReport.BuildCareerPathPlanReport("test", items, clock);
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
        CareerPathPlanReportCountsEachRecordOnce();
    }
}
