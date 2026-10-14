namespace Acme.Workforce.Workplace.Tests.Booking.Rollup;

using Acme.Workforce.Workplace.Booking.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class BookingRecordChecks
{
    public static void BookingRecordStartsInRequested()
    {
        var item = BookingRecord.NewBookingRecord("code-344", "label-759");
        Check.Equal(item.Status, BookingRecordStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BookingRecordMovesToGranted()
    {
        var item = BookingRecord.NewBookingRecord("code-344", "label-759");
        try
        {
            item.MoveTo(BookingRecordStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BookingRecordStatus.Granted, "status");
    }

    public static void BookingRecordRejectsMovesFromExpired()
    {
        var item = BookingRecord.NewBookingRecord("code-344", "label-759");
        item.Status = BookingRecordStatus.Expired;
        Check.Fails(() => item.MoveTo(BookingRecordStatus.Requested), "move");
    }

    public static void RunAll()
    {
        BookingRecordStartsInRequested();
        BookingRecordMovesToGranted();
        BookingRecordRejectsMovesFromExpired();
    }
}
