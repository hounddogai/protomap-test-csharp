namespace Acme.Workforce.Engagement.Tests.ActionPlan.Bundle;

using Acme.Workforce.Engagement.ActionPlan.Bundle;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class ActionPlanReservationReportChecks
{
    public static void ActionPlanReservationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700982524);
        var items = new List<ActionPlanReservation>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(ActionPlanReservation.NewActionPlanReservation($"action-plan-reservation-{index}", "sample"));
        }
        var report = ActionPlanReservationReport.BuildActionPlanReservationReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 6, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        ActionPlanReservationReportCountsEachRecordOnce();
    }
}
