namespace Acme.Workforce.Compensation.Tests.Adjustment.Tier;

using Acme.Workforce.Compensation.Adjustment.Tier;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class AdaptiveAdjustmentTargetReportChecks
{
    public static void AdaptiveAdjustmentTargetReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700887652);
        var items = new List<AdaptiveAdjustmentTarget>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(AdaptiveAdjustmentTarget.NewAdaptiveAdjustmentTarget($"adaptive-adjustment-target-{index}", "sample"));
        }
        var report = AdaptiveAdjustmentTargetReport.BuildAdaptiveAdjustmentTargetReport("test", items, clock);
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
        AdaptiveAdjustmentTargetReportCountsEachRecordOnce();
    }
}
