namespace Acme.Workforce.Orgdesign.Tests.Position.Segment;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Position.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class PositionSegmentReportChecks
{
    public static void PositionSegmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702301525);
        var items = new List<PositionSegment>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(PositionSegment.NewPositionSegment($"position-segment-{index}", "sample"));
        }
        var report = PositionSegmentReport.BuildPositionSegmentReport("test", items, clock);
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
        PositionSegmentReportCountsEachRecordOnce();
    }
}
