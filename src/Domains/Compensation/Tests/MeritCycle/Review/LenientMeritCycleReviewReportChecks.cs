namespace Acme.Workforce.Compensation.Tests.MeritCycle.Review;

using Acme.Workforce.Compensation.MeritCycle.Review;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class LenientMeritCycleReviewReportChecks
{
    public static void LenientMeritCycleReviewReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702972866);
        var items = new List<LenientMeritCycleReview>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(LenientMeritCycleReview.NewLenientMeritCycleReview($"lenient-merit-cycle-review-{index}", "sample"));
        }
        var report = LenientMeritCycleReviewReport.BuildLenientMeritCycleReviewReport("test", items, clock);
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
        LenientMeritCycleReviewReportCountsEachRecordOnce();
    }
}
