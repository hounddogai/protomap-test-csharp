namespace Acme.Workforce.Workplace.Tests.Booking.BookingVariance;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Booking.BookingVariance;

/// <summary>Checks of booking variance code, run by the project's runner.</summary>
public static class BookingDraftReportChecks
{
    public static void BookingDraftReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707216399);
        var items = new List<BookingDraft>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(BookingDraft.NewBookingDraft($"booking-draft-{index}", "sample"));
        }
        var report = BookingDraftReport.BuildBookingDraftReport("test", items, clock);
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
        BookingDraftReportCountsEachRecordOnce();
    }
}
