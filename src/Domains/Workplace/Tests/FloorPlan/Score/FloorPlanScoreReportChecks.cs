namespace Acme.Workforce.Workplace.Tests.FloorPlan.Score;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.FloorPlan.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class FloorPlanScoreReportChecks
{
    public static void FloorPlanScoreReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705284582);
        var items = new List<FloorPlanScore>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(FloorPlanScore.NewFloorPlanScore($"floor-plan-score-{index}", "sample"));
        }
        var report = FloorPlanScoreReport.BuildFloorPlanScoreReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 3, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        FloorPlanScoreReportCountsEachRecordOnce();
    }
}
