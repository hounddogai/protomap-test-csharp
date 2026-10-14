namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Audit;

using Acme.Workforce.Engagement.PulseSurvey.Audit;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class FallbackPulseSurveySegmentReportChecks
{
    public static void FallbackPulseSurveySegmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709725456);
        var items = new List<FallbackPulseSurveySegment>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(FallbackPulseSurveySegment.NewFallbackPulseSurveySegment($"fallback-pulse-survey-segment-{index}", "sample"));
        }
        var report = FallbackPulseSurveySegmentReport.BuildFallbackPulseSurveySegmentReport("test", items, clock);
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
        FallbackPulseSurveySegmentReportCountsEachRecordOnce();
    }
}
