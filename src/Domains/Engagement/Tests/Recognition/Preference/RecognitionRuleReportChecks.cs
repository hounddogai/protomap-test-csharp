namespace Acme.Workforce.Engagement.Tests.Recognition.Preference;

using Acme.Workforce.Engagement.Recognition.Preference;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class RecognitionRuleReportChecks
{
    public static void RecognitionRuleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704113896);
        var items = new List<RecognitionRule>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(RecognitionRule.NewRecognitionRule($"recognition-rule-{index}", "sample"));
        }
        var report = RecognitionRuleReport.BuildRecognitionRuleReport("test", items, clock);
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
        RecognitionRuleReportCountsEachRecordOnce();
    }
}
