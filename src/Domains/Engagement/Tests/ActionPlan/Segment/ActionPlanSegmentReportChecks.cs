namespace Acme.Workforce.Engagement.Tests.ActionPlan.Segment;

using Acme.Workforce.Engagement.ActionPlan.Segment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class ActionPlanSegmentReportChecks
{
    public static void ActionPlanSegmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707281007);
        var items = new List<ActionPlanSegment>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(ActionPlanSegment.NewActionPlanSegment($"action-plan-segment-{index}", "sample"));
        }
        var report = ActionPlanSegmentReport.BuildActionPlanSegmentReport("test", items, clock);
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
        ActionPlanSegmentReportCountsEachRecordOnce();
    }
}
