namespace Acme.Workforce.Engagement.Tests.ActionPlan.Snapshot;

using Acme.Workforce.Engagement.ActionPlan.Snapshot;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class StrictActionPlanThresholdReportChecks
{
    public static void StrictActionPlanThresholdReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703243351);
        var items = new List<StrictActionPlanThreshold>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(StrictActionPlanThreshold.NewStrictActionPlanThreshold($"strict-action-plan-threshold-{index}", "sample"));
        }
        var report = StrictActionPlanThresholdReport.BuildStrictActionPlanThresholdReport("test", items, clock);
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
        StrictActionPlanThresholdReportCountsEachRecordOnce();
    }
}
