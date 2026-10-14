namespace Acme.Workforce.Engagement.Tests.Sentiment.Rollup;

using Acme.Workforce.Engagement.Sentiment.Rollup;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class ManualSentimentAdjustmentReportChecks
{
    public static void ManualSentimentAdjustmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706878738);
        var items = new List<ManualSentimentAdjustment>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(ManualSentimentAdjustment.NewManualSentimentAdjustment($"manual-sentiment-adjustment-{index}", "sample"));
        }
        var report = ManualSentimentAdjustmentReport.BuildManualSentimentAdjustmentReport("test", items, clock);
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
        ManualSentimentAdjustmentReportCountsEachRecordOnce();
    }
}
