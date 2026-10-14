namespace Acme.Workforce.Compensation.Tests.Adjustment.Score;

using Acme.Workforce.Compensation.Adjustment.Score;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class AdjustmentPlanReportChecks
{
    public static void AdjustmentPlanReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705063773);
        var items = new List<AdjustmentPlan>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(AdjustmentPlan.NewAdjustmentPlan($"adjustment-plan-{index}", "sample"));
        }
        var report = AdjustmentPlanReport.BuildAdjustmentPlanReport("test", items, clock);
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
        AdjustmentPlanReportCountsEachRecordOnce();
    }
}
