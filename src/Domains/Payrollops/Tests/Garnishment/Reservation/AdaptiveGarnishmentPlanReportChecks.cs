namespace Acme.Workforce.Payrollops.Tests.Garnishment.Reservation;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Garnishment.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class AdaptiveGarnishmentPlanReportChecks
{
    public static void AdaptiveGarnishmentPlanReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705787029);
        var items = new List<AdaptiveGarnishmentPlan>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(AdaptiveGarnishmentPlan.NewAdaptiveGarnishmentPlan($"adaptive-garnishment-plan-{index}", "sample"));
        }
        var report = AdaptiveGarnishmentPlanReport.BuildAdaptiveGarnishmentPlanReport("test", items, clock);
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
        AdaptiveGarnishmentPlanReportCountsEachRecordOnce();
    }
}
