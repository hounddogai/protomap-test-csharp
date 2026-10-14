namespace Acme.Workforce.Workplace.Tests.Booking.Variance;

using Acme.Workforce.Workplace.Booking.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class BookingExceptionChecks
{
    public static void BookingExceptionStartsInProposed()
    {
        var item = BookingException.NewBookingException("code-763", "label-866");
        Check.Equal(item.Status, BookingExceptionStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BookingExceptionMovesToApproved()
    {
        var item = BookingException.NewBookingException("code-763", "label-866");
        try
        {
            item.MoveTo(BookingExceptionStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BookingExceptionStatus.Approved, "status");
    }

    public static void BookingExceptionRejectsMovesFromWithdrawn()
    {
        var item = BookingException.NewBookingException("code-763", "label-866");
        item.Status = BookingExceptionStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(BookingExceptionStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        BookingExceptionStartsInProposed();
        BookingExceptionMovesToApproved();
        BookingExceptionRejectsMovesFromWithdrawn();
    }
}
