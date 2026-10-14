namespace Acme.Workforce.Compliance.Tests.Audit.Review;

using Acme.Workforce.Compliance.Audit.Review;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class AuditReviewReportChecks
{
    public static void AuditReviewReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704543579);
        var items = new List<AuditReview>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(AuditReview.NewAuditReview($"audit-review-{index}", "sample"));
        }
        var report = AuditReviewReport.BuildAuditReviewReport("test", items, clock);
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
        AuditReviewReportCountsEachRecordOnce();
    }
}
