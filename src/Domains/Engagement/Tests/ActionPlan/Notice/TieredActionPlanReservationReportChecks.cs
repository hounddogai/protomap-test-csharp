namespace Acme.Workforce.Engagement.Tests.ActionPlan.Notice;

using Acme.Workforce.Engagement.ActionPlan.Notice;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class TieredActionPlanReservationReportChecks
{
    public static void TieredActionPlanReservationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709213480);
        var items = new List<TieredActionPlanReservation>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(TieredActionPlanReservation.NewTieredActionPlanReservation($"tiered-action-plan-reservation-{index}", "sample"));
        }
        var report = TieredActionPlanReservationReport.BuildTieredActionPlanReservationReport("test", items, clock);
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
        TieredActionPlanReservationReportCountsEachRecordOnce();
    }
}
