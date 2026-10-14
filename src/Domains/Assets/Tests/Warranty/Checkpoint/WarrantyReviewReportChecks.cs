namespace Acme.Workforce.Assets.Tests.Warranty.Checkpoint;

using Acme.Workforce.Assets.Warranty.Checkpoint;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class WarrantyReviewReportChecks
{
    public static void WarrantyReviewReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705318042);
        var items = new List<WarrantyReview>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(WarrantyReview.NewWarrantyReview($"warranty-review-{index}", "sample"));
        }
        var report = WarrantyReviewReport.BuildWarrantyReviewReport("test", items, clock);
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
        WarrantyReviewReportCountsEachRecordOnce();
    }
}
