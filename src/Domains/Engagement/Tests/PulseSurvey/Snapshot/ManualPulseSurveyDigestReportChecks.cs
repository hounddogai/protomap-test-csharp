namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Snapshot;

using Acme.Workforce.Engagement.PulseSurvey.Snapshot;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class ManualPulseSurveyDigestReportChecks
{
    public static void ManualPulseSurveyDigestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702300348);
        var items = new List<ManualPulseSurveyDigest>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(ManualPulseSurveyDigest.NewManualPulseSurveyDigest($"manual-pulse-survey-digest-{index}", "sample"));
        }
        var report = ManualPulseSurveyDigestReport.BuildManualPulseSurveyDigestReport("test", items, clock);
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
        ManualPulseSurveyDigestReportCountsEachRecordOnce();
    }
}
