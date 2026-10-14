namespace Acme.Workforce.Workplace.Tests.Booking.Entry;

using Acme.Workforce.Workplace.Booking.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class TieredBookingRecordViewChecks
{
    public static void TieredBookingRecordViewShowsTheStatusInCapitals()
    {
        var item = TieredBookingRecord.NewTieredBookingRecord("code-399", "name-393");
        var view = TieredBookingRecordView.ToTieredBookingRecordView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TieredBookingRecordView.ToTieredBookingRecordViews(new List<TieredBookingRecord> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TieredBookingRecordViewShowsTheStatusInCapitals();
    }
}
