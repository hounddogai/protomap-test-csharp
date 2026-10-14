namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Variance;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.SuccessionPlan.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class SuccessionPlanVarianceReportChecks
{
    public static void SuccessionPlanVarianceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706996412);
        var items = new List<SuccessionPlanVariance>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(SuccessionPlanVariance.NewSuccessionPlanVariance($"succession-plan-variance-{index}", "sample"));
        }
        var report = SuccessionPlanVarianceReport.BuildSuccessionPlanVarianceReport("test", items, clock);
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
        SuccessionPlanVarianceReportCountsEachRecordOnce();
    }
}
