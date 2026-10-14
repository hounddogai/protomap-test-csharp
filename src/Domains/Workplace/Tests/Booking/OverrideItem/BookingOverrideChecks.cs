namespace Acme.Workforce.Workplace.Tests.Booking.OverrideItem;

using Acme.Workforce.Workplace.Booking.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class BookingOverrideChecks
{
    public static void BookingOverrideStartsInCreated()
    {
        var item = BookingOverride.NewBookingOverride("code-408", "label-900");
        Check.Equal(item.Status, BookingOverrideStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BookingOverrideMovesToPublished()
    {
        var item = BookingOverride.NewBookingOverride("code-408", "label-900");
        try
        {
            item.MoveTo(BookingOverrideStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BookingOverrideStatus.Published, "status");
    }

    public static void BookingOverrideRejectsMovesFromRetired()
    {
        var item = BookingOverride.NewBookingOverride("code-408", "label-900");
        item.Status = BookingOverrideStatus.Retired;
        Check.Fails(() => item.MoveTo(BookingOverrideStatus.Created), "move");
    }

    public static void RunAll()
    {
        BookingOverrideStartsInCreated();
        BookingOverrideMovesToPublished();
        BookingOverrideRejectsMovesFromRetired();
    }
}
