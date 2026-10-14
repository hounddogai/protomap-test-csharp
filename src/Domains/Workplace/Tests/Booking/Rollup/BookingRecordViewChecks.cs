namespace Acme.Workforce.Workplace.Tests.Booking.Rollup;

using Acme.Workforce.Workplace.Booking.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class BookingRecordViewChecks
{
    public static void BookingRecordViewShowsTheStatusInCapitals()
    {
        var item = BookingRecord.NewBookingRecord("code-344", "label-759");
        var view = BookingRecordView.ToBookingRecordView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BookingRecordView.ToBookingRecordViews(new List<BookingRecord> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BookingRecordViewShowsTheStatusInCapitals();
    }
}
