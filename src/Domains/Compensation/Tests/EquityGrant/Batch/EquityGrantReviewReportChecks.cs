namespace Acme.Workforce.Compensation.Tests.EquityGrant.Batch;

using Acme.Workforce.Compensation.EquityGrant.Batch;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class EquityGrantReviewReportChecks
{
    public static void EquityGrantReviewReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708161653);
        var items = new List<EquityGrantReview>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(EquityGrantReview.NewEquityGrantReview($"equity-grant-review-{index}", "sample"));
        }
        var report = EquityGrantReviewReport.BuildEquityGrantReviewReport("test", items, clock);
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
        EquityGrantReviewReportCountsEachRecordOnce();
    }
}
