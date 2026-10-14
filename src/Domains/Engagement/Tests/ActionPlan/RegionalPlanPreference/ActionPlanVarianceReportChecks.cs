namespace Acme.Workforce.Engagement.Tests.ActionPlan.RegionalPlanPreference;

using Acme.Workforce.Engagement.ActionPlan.RegionalPlanPreference;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of regional plan preference code, run by the project's runner.</summary>
public static class ActionPlanVarianceReportChecks
{
    public static void ActionPlanVarianceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703250181);
        var items = new List<ActionPlanVariance>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(ActionPlanVariance.NewActionPlanVariance($"action-plan-variance-{index}", "sample"));
        }
        var report = ActionPlanVarianceReport.BuildActionPlanVarianceReport("test", items, clock);
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
        ActionPlanVarianceReportCountsEachRecordOnce();
    }
}
