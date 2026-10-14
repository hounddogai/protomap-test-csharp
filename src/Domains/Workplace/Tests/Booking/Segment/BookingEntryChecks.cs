namespace Acme.Workforce.Workplace.Tests.Booking.Segment;

using Acme.Workforce.Workplace.Booking.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class BookingEntryChecks
{
    public static void BookingEntryStartsInProposed()
    {
        var item = BookingEntry.NewBookingEntry("code-623", "title-581");
        Check.Equal(item.Status, BookingEntryStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BookingEntryMovesToApproved()
    {
        var item = BookingEntry.NewBookingEntry("code-623", "title-581");
        try
        {
            item.MoveTo(BookingEntryStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BookingEntryStatus.Approved, "status");
    }

    public static void BookingEntryRejectsMovesFromWithdrawn()
    {
        var item = BookingEntry.NewBookingEntry("code-623", "title-581");
        item.Status = BookingEntryStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(BookingEntryStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        BookingEntryStartsInProposed();
        BookingEntryMovesToApproved();
        BookingEntryRejectsMovesFromWithdrawn();
    }
}
