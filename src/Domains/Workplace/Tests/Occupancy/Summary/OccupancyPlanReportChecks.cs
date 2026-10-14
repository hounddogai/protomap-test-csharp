namespace Acme.Workforce.Workplace.Tests.Occupancy.Summary;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Occupancy.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class OccupancyPlanReportChecks
{
    public static void OccupancyPlanReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701333467);
        var items = new List<OccupancyPlan>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(OccupancyPlan.NewOccupancyPlan($"occupancy-plan-{index}", "sample"));
        }
        var report = OccupancyPlanReport.BuildOccupancyPlanReport("test", items, clock);
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
        OccupancyPlanReportCountsEachRecordOnce();
    }
}
