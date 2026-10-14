namespace Acme.Workforce.Compensation.Tests.Adjustment.Template;

using Acme.Workforce.Compensation.Adjustment.Template;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class ScheduledAdjustmentSummaryReportChecks
{
    public static void ScheduledAdjustmentSummaryReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702009175);
        var items = new List<ScheduledAdjustmentSummary>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(ScheduledAdjustmentSummary.NewScheduledAdjustmentSummary($"scheduled-adjustment-summary-{index}", "sample"));
        }
        var report = ScheduledAdjustmentSummaryReport.BuildScheduledAdjustmentSummaryReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 6, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        ScheduledAdjustmentSummaryReportCountsEachRecordOnce();
    }
}
