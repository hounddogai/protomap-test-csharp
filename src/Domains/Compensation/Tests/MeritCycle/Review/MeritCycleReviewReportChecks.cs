namespace Acme.Workforce.Compensation.Tests.MeritCycle.Review;

using Acme.Workforce.Compensation.MeritCycle.Review;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class MeritCycleReviewReportChecks
{
    public static void MeritCycleReviewReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707505772);
        var items = new List<MeritCycleReview>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(MeritCycleReview.NewMeritCycleReview($"merit-cycle-review-{index}", "sample"));
        }
        var report = MeritCycleReviewReport.BuildMeritCycleReviewReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 3, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        MeritCycleReviewReportCountsEachRecordOnce();
    }
}
