namespace Acme.Workforce.Orgdesign.Tests.Span.Preference;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Span.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class SpanReviewReportChecks
{
    public static void SpanReviewReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700864997);
        var items = new List<SpanReview>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(SpanReview.NewSpanReview($"span-review-{index}", "sample"));
        }
        var report = SpanReviewReport.BuildSpanReviewReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 2, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        SpanReviewReportCountsEachRecordOnce();
    }
}
