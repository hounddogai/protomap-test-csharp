namespace Acme.Workforce.Workplace.Tests.Booking.Baseline;

using Acme.Workforce.Workplace.Booking.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class BookingBaselineChecks
{
    public static void BookingBaselineStartsInDraft()
    {
        var item = BookingBaseline.NewBookingBaseline("code-253", "label-906");
        Check.Equal(item.Status, BookingBaselineStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BookingBaselineMovesToPending()
    {
        var item = BookingBaseline.NewBookingBaseline("code-253", "label-906");
        try
        {
            item.MoveTo(BookingBaselineStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BookingBaselineStatus.Pending, "status");
    }

    public static void BookingBaselineRejectsMovesFromClosed()
    {
        var item = BookingBaseline.NewBookingBaseline("code-253", "label-906");
        item.Status = BookingBaselineStatus.Closed;
        Check.Fails(() => item.MoveTo(BookingBaselineStatus.Draft), "move");
    }

    public static void RunAll()
    {
        BookingBaselineStartsInDraft();
        BookingBaselineMovesToPending();
        BookingBaselineRejectsMovesFromClosed();
    }
}
