namespace Acme.Workforce.Workplace.Tests.Booking.Window;

using Acme.Workforce.Workplace.Booking.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class BookingTierChecks
{
    public static void BookingTierStartsInOpen()
    {
        var item = BookingTier.NewBookingTier("code-783", "name-449");
        Check.Equal(item.Status, BookingTierStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BookingTierMovesToResolved()
    {
        var item = BookingTier.NewBookingTier("code-783", "name-449");
        try
        {
            item.MoveTo(BookingTierStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BookingTierStatus.Resolved, "status");
    }

    public static void BookingTierRejectsMovesFromArchived()
    {
        var item = BookingTier.NewBookingTier("code-783", "name-449");
        item.Status = BookingTierStatus.Archived;
        Check.Fails(() => item.MoveTo(BookingTierStatus.Open), "move");
    }

    public static void RunAll()
    {
        BookingTierStartsInOpen();
        BookingTierMovesToResolved();
        BookingTierRejectsMovesFromArchived();
    }
}
