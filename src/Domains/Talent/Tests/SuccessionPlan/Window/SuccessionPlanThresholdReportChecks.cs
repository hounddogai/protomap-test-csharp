namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Window;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.SuccessionPlan.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class SuccessionPlanThresholdReportChecks
{
    public static void SuccessionPlanThresholdReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702714175);
        var items = new List<SuccessionPlanThreshold>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(SuccessionPlanThreshold.NewSuccessionPlanThreshold($"succession-plan-threshold-{index}", "sample"));
        }
        var report = SuccessionPlanThresholdReport.BuildSuccessionPlanThresholdReport("test", items, clock);
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
        SuccessionPlanThresholdReportCountsEachRecordOnce();
    }
}
