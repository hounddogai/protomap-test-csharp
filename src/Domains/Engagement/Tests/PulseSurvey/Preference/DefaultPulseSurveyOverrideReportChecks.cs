namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Preference;

using Acme.Workforce.Engagement.PulseSurvey.Preference;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class DefaultPulseSurveyOverrideReportChecks
{
    public static void DefaultPulseSurveyOverrideReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709159128);
        var items = new List<DefaultPulseSurveyOverride>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(DefaultPulseSurveyOverride.NewDefaultPulseSurveyOverride($"default-pulse-survey-override-{index}", "sample"));
        }
        var report = DefaultPulseSurveyOverrideReport.BuildDefaultPulseSurveyOverrideReport("test", items, clock);
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
        DefaultPulseSurveyOverrideReportCountsEachRecordOnce();
    }
}
