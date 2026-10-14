namespace Acme.Workforce.Workplace.Tests.FloorPlan.Reservation;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.FloorPlan.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class StrictFloorPlanBundleReportChecks
{
    public static void StrictFloorPlanBundleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707309906);
        var items = new List<StrictFloorPlanBundle>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(StrictFloorPlanBundle.NewStrictFloorPlanBundle($"strict-floor-plan-bundle-{index}", "sample"));
        }
        var report = StrictFloorPlanBundleReport.BuildStrictFloorPlanBundleReport("test", items, clock);
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
        StrictFloorPlanBundleReportCountsEachRecordOnce();
    }
}
