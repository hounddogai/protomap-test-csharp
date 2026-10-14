namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Threshold;

using Acme.Workforce.Engagement.PulseSurvey.Threshold;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class PulseSurveyReviewReportChecks
{
    public static void PulseSurveyReviewReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706327435);
        var items = new List<PulseSurveyReview>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(PulseSurveyReview.NewPulseSurveyReview($"pulse-survey-review-{index}", "sample"));
        }
        var report = PulseSurveyReviewReport.BuildPulseSurveyReviewReport("test", items, clock);
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
        PulseSurveyReviewReportCountsEachRecordOnce();
    }
}
