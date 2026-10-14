namespace Acme.Workforce.Orgdesign.Tests.Position.Threshold;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Position.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class PendingPositionReviewReportChecks
{
    public static void PendingPositionReviewReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709610462);
        var items = new List<PendingPositionReview>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(PendingPositionReview.NewPendingPositionReview($"pending-position-review-{index}", "sample"));
        }
        var report = PendingPositionReviewReport.BuildPendingPositionReviewReport("test", items, clock);
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
        PendingPositionReviewReportCountsEachRecordOnce();
    }
}
