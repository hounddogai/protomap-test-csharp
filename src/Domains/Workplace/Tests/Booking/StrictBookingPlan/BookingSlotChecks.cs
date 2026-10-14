namespace Acme.Workforce.Workplace.Tests.Booking.StrictBookingPlan;

using Acme.Workforce.Workplace.Booking.StrictBookingPlan;

/// <summary>Checks of strict booking plan code, run by the project's runner.</summary>
public static class BookingSlotChecks
{
    public static void BookingSlotStartsInCreated()
    {
        var item = BookingSlot.NewBookingSlot("code-704", "title-947");
        Check.Equal(item.Status, BookingSlotStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BookingSlotMovesToRetired()
    {
        var item = BookingSlot.NewBookingSlot("code-704", "title-947");
        try
        {
            item.MoveTo(BookingSlotStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BookingSlotStatus.Retired, "status");
    }

    public static void BookingSlotRejectsMovesFromRetired()
    {
        var item = BookingSlot.NewBookingSlot("code-704", "title-947");
        item.Status = BookingSlotStatus.Retired;
        Check.Fails(() => item.MoveTo(BookingSlotStatus.Created), "move");
    }

    public static void RunAll()
    {
        BookingSlotStartsInCreated();
        BookingSlotMovesToRetired();
        BookingSlotRejectsMovesFromRetired();
    }
}
