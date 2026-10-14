namespace Acme.Workforce.Talent.Tests.SuccessionPlan.PlanVariance;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.SuccessionPlan.PlanVariance;

/// <summary>Checks of plan variance code, run by the project's runner.</summary>
public static class DefaultSuccessionPlanVarianceReportChecks
{
    public static void DefaultSuccessionPlanVarianceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702067672);
        var items = new List<DefaultSuccessionPlanVariance>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(DefaultSuccessionPlanVariance.NewDefaultSuccessionPlanVariance($"default-succession-plan-variance-{index}", "sample"));
        }
        var report = DefaultSuccessionPlanVarianceReport.BuildDefaultSuccessionPlanVarianceReport("test", items, clock);
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
        DefaultSuccessionPlanVarianceReportCountsEachRecordOnce();
    }
}
