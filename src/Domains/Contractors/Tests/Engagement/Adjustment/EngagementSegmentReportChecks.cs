namespace Acme.Workforce.Contractors.Tests.Engagement.Adjustment;

using Acme.Workforce.Contractors.Engagement.Adjustment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class EngagementSegmentReportChecks
{
    public static void EngagementSegmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701071657);
        var items = new List<EngagementSegment>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(EngagementSegment.NewEngagementSegment($"engagement-segment-{index}", "sample"));
        }
        var report = EngagementSegmentReport.BuildEngagementSegmentReport("test", items, clock);
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
        EngagementSegmentReportCountsEachRecordOnce();
    }
}
