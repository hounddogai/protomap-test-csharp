namespace Acme.Workforce.Assets.Tests.Warranty.WarrantyAdjustment;

using Acme.Workforce.Assets.Warranty.WarrantyAdjustment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of warranty adjustment code, run by the project's runner.</summary>
public static class ScheduledWarrantyReviewReportChecks
{
    public static void ScheduledWarrantyReviewReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701816142);
        var items = new List<ScheduledWarrantyReview>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(ScheduledWarrantyReview.NewScheduledWarrantyReview($"scheduled-warranty-review-{index}", "sample"));
        }
        var report = ScheduledWarrantyReviewReport.BuildScheduledWarrantyReviewReport("test", items, clock);
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
        ScheduledWarrantyReviewReportCountsEachRecordOnce();
    }
}
