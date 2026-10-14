namespace Acme.Workforce.Workplace.Tests.Booking.Audit;

using Acme.Workforce.Workplace.Booking.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class BookingProfileChecks
{
    public static void BookingProfileStartsInRequested()
    {
        var item = BookingProfile.NewBookingProfile("code-249", "label-779");
        Check.Equal(item.Status, BookingProfileStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BookingProfileMovesToGranted()
    {
        var item = BookingProfile.NewBookingProfile("code-249", "label-779");
        try
        {
            item.MoveTo(BookingProfileStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BookingProfileStatus.Granted, "status");
    }

    public static void BookingProfileRejectsMovesFromExpired()
    {
        var item = BookingProfile.NewBookingProfile("code-249", "label-779");
        item.Status = BookingProfileStatus.Expired;
        Check.Fails(() => item.MoveTo(BookingProfileStatus.Requested), "move");
    }

    public static void RunAll()
    {
        BookingProfileStartsInRequested();
        BookingProfileMovesToGranted();
        BookingProfileRejectsMovesFromExpired();
    }
}
