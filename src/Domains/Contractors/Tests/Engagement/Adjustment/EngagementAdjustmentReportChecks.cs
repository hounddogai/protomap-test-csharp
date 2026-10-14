namespace Acme.Workforce.Contractors.Tests.Engagement.Adjustment;

using Acme.Workforce.Contractors.Engagement.Adjustment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class EngagementAdjustmentReportChecks
{
    public static void EngagementAdjustmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706611010);
        var items = new List<EngagementAdjustment>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(EngagementAdjustment.NewEngagementAdjustment($"engagement-adjustment-{index}", "sample"));
        }
        var report = EngagementAdjustmentReport.BuildEngagementAdjustmentReport("test", items, clock);
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
        EngagementAdjustmentReportCountsEachRecordOnce();
    }
}
