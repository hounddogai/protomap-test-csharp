namespace Acme.Workforce.Compensation.Tests.Adjustment.AdjustmentRule;

using Acme.Workforce.Compensation.Adjustment.AdjustmentRule;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of adjustment rule code, run by the project's runner.</summary>
public static class AdaptiveAdjustmentNoticeReportChecks
{
    public static void AdaptiveAdjustmentNoticeReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701330904);
        var items = new List<AdaptiveAdjustmentNotice>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(AdaptiveAdjustmentNotice.NewAdaptiveAdjustmentNotice($"adaptive-adjustment-notice-{index}", "sample"));
        }
        var report = AdaptiveAdjustmentNoticeReport.BuildAdaptiveAdjustmentNoticeReport("test", items, clock);
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
        AdaptiveAdjustmentNoticeReportCountsEachRecordOnce();
    }
}
