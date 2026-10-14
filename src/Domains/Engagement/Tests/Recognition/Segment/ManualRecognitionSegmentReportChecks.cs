namespace Acme.Workforce.Engagement.Tests.Recognition.Segment;

using Acme.Workforce.Engagement.Recognition.Segment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class ManualRecognitionSegmentReportChecks
{
    public static void ManualRecognitionSegmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707784091);
        var items = new List<ManualRecognitionSegment>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(ManualRecognitionSegment.NewManualRecognitionSegment($"manual-recognition-segment-{index}", "sample"));
        }
        var report = ManualRecognitionSegmentReport.BuildManualRecognitionSegmentReport("test", items, clock);
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
        ManualRecognitionSegmentReportCountsEachRecordOnce();
    }
}
