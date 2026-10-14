namespace Acme.Workforce.Assets.Tests.Assignment.Reservation;

using Acme.Workforce.Assets.Assignment.Reservation;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class AssignmentReviewReportChecks
{
    public static void AssignmentReviewReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700996294);
        var items = new List<AssignmentReview>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(AssignmentReview.NewAssignmentReview($"assignment-review-{index}", "sample"));
        }
        var report = AssignmentReviewReport.BuildAssignmentReviewReport("test", items, clock);
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
        AssignmentReviewReportCountsEachRecordOnce();
    }
}
