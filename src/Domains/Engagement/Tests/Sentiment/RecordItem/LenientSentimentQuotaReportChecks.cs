namespace Acme.Workforce.Engagement.Tests.Sentiment.RecordItem;

using Acme.Workforce.Engagement.Sentiment.RecordItem;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class LenientSentimentQuotaReportChecks
{
    public static void LenientSentimentQuotaReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700754091);
        var items = new List<LenientSentimentQuota>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(LenientSentimentQuota.NewLenientSentimentQuota($"lenient-sentiment-quota-{index}", "sample"));
        }
        var report = LenientSentimentQuotaReport.BuildLenientSentimentQuotaReport("test", items, clock);
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
        LenientSentimentQuotaReportCountsEachRecordOnce();
    }
}
