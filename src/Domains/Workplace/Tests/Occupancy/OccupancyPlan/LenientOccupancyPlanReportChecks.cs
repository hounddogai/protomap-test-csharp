namespace Acme.Workforce.Workplace.Tests.Occupancy.OccupancyPlan;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Occupancy.OccupancyPlan;

/// <summary>Checks of occupancy plan code, run by the project's runner.</summary>
public static class LenientOccupancyPlanReportChecks
{
    public static void LenientOccupancyPlanReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706997943);
        var items = new List<LenientOccupancyPlan>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(LenientOccupancyPlan.NewLenientOccupancyPlan($"lenient-occupancy-plan-{index}", "sample"));
        }
        var report = LenientOccupancyPlanReport.BuildLenientOccupancyPlanReport("test", items, clock);
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
        LenientOccupancyPlanReportCountsEachRecordOnce();
    }
}
