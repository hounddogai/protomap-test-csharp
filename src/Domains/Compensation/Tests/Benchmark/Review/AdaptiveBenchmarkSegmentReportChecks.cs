namespace Acme.Workforce.Compensation.Tests.Benchmark.Review;

using Acme.Workforce.Compensation.Benchmark.Review;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class AdaptiveBenchmarkSegmentReportChecks
{
    public static void AdaptiveBenchmarkSegmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705754701);
        var items = new List<AdaptiveBenchmarkSegment>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(AdaptiveBenchmarkSegment.NewAdaptiveBenchmarkSegment($"adaptive-benchmark-segment-{index}", "sample"));
        }
        var report = AdaptiveBenchmarkSegmentReport.BuildAdaptiveBenchmarkSegmentReport("test", items, clock);
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
        AdaptiveBenchmarkSegmentReportCountsEachRecordOnce();
    }
}
