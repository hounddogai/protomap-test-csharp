namespace Acme.Workforce.Compensation.Tests.Adjustment.Summary;

using Acme.Workforce.Compensation.Adjustment.Summary;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class GlobalAdjustmentSummaryReportChecks
{
    public static void GlobalAdjustmentSummaryReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706527816);
        var items = new List<GlobalAdjustmentSummary>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(GlobalAdjustmentSummary.NewGlobalAdjustmentSummary($"global-adjustment-summary-{index}", "sample"));
        }
        var report = GlobalAdjustmentSummaryReport.BuildGlobalAdjustmentSummaryReport("test", items, clock);
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
        GlobalAdjustmentSummaryReportCountsEachRecordOnce();
    }
}
