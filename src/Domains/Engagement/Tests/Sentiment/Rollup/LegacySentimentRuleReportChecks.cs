namespace Acme.Workforce.Engagement.Tests.Sentiment.Rollup;

using Acme.Workforce.Engagement.Sentiment.Rollup;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class LegacySentimentRuleReportChecks
{
    public static void LegacySentimentRuleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702499485);
        var items = new List<LegacySentimentRule>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(LegacySentimentRule.NewLegacySentimentRule($"legacy-sentiment-rule-{index}", "sample"));
        }
        var report = LegacySentimentRuleReport.BuildLegacySentimentRuleReport("test", items, clock);
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
        LegacySentimentRuleReportCountsEachRecordOnce();
    }
}
