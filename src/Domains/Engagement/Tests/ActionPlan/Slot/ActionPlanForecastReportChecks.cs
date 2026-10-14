namespace Acme.Workforce.Engagement.Tests.ActionPlan.Slot;

using Acme.Workforce.Engagement.ActionPlan.Slot;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class ActionPlanForecastReportChecks
{
    public static void ActionPlanForecastReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700333338);
        var items = new List<ActionPlanForecast>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(ActionPlanForecast.NewActionPlanForecast($"action-plan-forecast-{index}", "sample"));
        }
        var report = ActionPlanForecastReport.BuildActionPlanForecastReport("test", items, clock);
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
        ActionPlanForecastReportCountsEachRecordOnce();
    }
}
