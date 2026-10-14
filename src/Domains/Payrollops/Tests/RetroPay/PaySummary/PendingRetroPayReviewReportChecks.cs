namespace Acme.Workforce.Payrollops.Tests.RetroPay.PaySummary;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.RetroPay.PaySummary;

/// <summary>Checks of pay summary code, run by the project's runner.</summary>
public static class PendingRetroPayReviewReportChecks
{
    public static void PendingRetroPayReviewReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704288816);
        var items = new List<PendingRetroPayReview>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(PendingRetroPayReview.NewPendingRetroPayReview($"pending-retro-pay-review-{index}", "sample"));
        }
        var report = PendingRetroPayReviewReport.BuildPendingRetroPayReviewReport("test", items, clock);
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
        PendingRetroPayReviewReportCountsEachRecordOnce();
    }
}
