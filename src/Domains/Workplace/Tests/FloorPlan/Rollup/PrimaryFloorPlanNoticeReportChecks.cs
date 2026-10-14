namespace Acme.Workforce.Workplace.Tests.FloorPlan.Rollup;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.FloorPlan.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class PrimaryFloorPlanNoticeReportChecks
{
    public static void PrimaryFloorPlanNoticeReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707434646);
        var items = new List<PrimaryFloorPlanNotice>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(PrimaryFloorPlanNotice.NewPrimaryFloorPlanNotice($"primary-floor-plan-notice-{index}", "sample"));
        }
        var report = PrimaryFloorPlanNoticeReport.BuildPrimaryFloorPlanNoticeReport("test", items, clock);
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
        PrimaryFloorPlanNoticeReportCountsEachRecordOnce();
    }
}
