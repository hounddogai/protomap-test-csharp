namespace Acme.Workforce.Workplace.Tests.Booking.Audit;

using Acme.Workforce.Workplace.Booking.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class BookingPreferenceChecks
{
    public static void BookingPreferenceStartsInIdle()
    {
        var item = BookingPreference.NewBookingPreference("code-246", "label-286");
        Check.Equal(item.Status, BookingPreferenceStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BookingPreferenceMovesToReady()
    {
        var item = BookingPreference.NewBookingPreference("code-246", "label-286");
        try
        {
            item.MoveTo(BookingPreferenceStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BookingPreferenceStatus.Ready, "status");
    }

    public static void BookingPreferenceRejectsMovesFromStopped()
    {
        var item = BookingPreference.NewBookingPreference("code-246", "label-286");
        item.Status = BookingPreferenceStatus.Stopped;
        Check.Fails(() => item.MoveTo(BookingPreferenceStatus.Idle), "move");
    }

    public static void RunAll()
    {
        BookingPreferenceStartsInIdle();
        BookingPreferenceMovesToReady();
        BookingPreferenceRejectsMovesFromStopped();
    }
}
