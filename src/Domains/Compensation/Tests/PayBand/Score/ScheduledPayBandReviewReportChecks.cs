namespace Acme.Workforce.Compensation.Tests.PayBand.Score;

using Acme.Workforce.Compensation.PayBand.Score;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class ScheduledPayBandReviewReportChecks
{
    public static void ScheduledPayBandReviewReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709515373);
        var items = new List<ScheduledPayBandReview>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(ScheduledPayBandReview.NewScheduledPayBandReview($"scheduled-pay-band-review-{index}", "sample"));
        }
        var report = ScheduledPayBandReviewReport.BuildScheduledPayBandReviewReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 6, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        ScheduledPayBandReviewReportCountsEachRecordOnce();
    }
}
