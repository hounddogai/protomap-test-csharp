namespace Acme.Workforce.Engagement.Tests.Recognition.Baseline;

using Acme.Workforce.Engagement.Recognition.Baseline;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class RecognitionMetricReportChecks
{
    public static void RecognitionMetricReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706769753);
        var items = new List<RecognitionMetric>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(RecognitionMetric.NewRecognitionMetric($"recognition-metric-{index}", "sample"));
        }
        var report = RecognitionMetricReport.BuildRecognitionMetricReport("test", items, clock);
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
        RecognitionMetricReportCountsEachRecordOnce();
    }
}
