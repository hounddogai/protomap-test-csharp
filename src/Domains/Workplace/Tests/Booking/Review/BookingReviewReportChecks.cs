namespace Acme.Workforce.Workplace.Tests.Booking.Review;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Booking.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class BookingReviewReportChecks
{
    public static void BookingReviewReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701683610);
        var items = new List<BookingReview>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(BookingReview.NewBookingReview($"booking-review-{index}", "sample"));
        }
        var report = BookingReviewReport.BuildBookingReviewReport("test", items, clock);
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
        BookingReviewReportCountsEachRecordOnce();
    }
}
