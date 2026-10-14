namespace Acme.Workforce.Workplace.Tests.FloorPlan.Audit;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.FloorPlan.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class FallbackFloorPlanRequestReportChecks
{
    public static void FallbackFloorPlanRequestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703454490);
        var items = new List<FallbackFloorPlanRequest>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(FallbackFloorPlanRequest.NewFallbackFloorPlanRequest($"fallback-floor-plan-request-{index}", "sample"));
        }
        var report = FallbackFloorPlanRequestReport.BuildFallbackFloorPlanRequestReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 4, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        FallbackFloorPlanRequestReportCountsEachRecordOnce();
    }
}
