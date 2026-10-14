namespace Acme.Workforce.Workplace.Tests.Booking.Reservation;

using Acme.Workforce.Workplace.Booking.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class BookingRequestChecks
{
    public static void BookingRequestStartsInOpen()
    {
        var item = BookingRequest.NewBookingRequest("code-336", "title-371");
        Check.Equal(item.Status, BookingRequestStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BookingRequestMovesToReopened()
    {
        var item = BookingRequest.NewBookingRequest("code-336", "title-371");
        try
        {
            item.MoveTo(BookingRequestStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BookingRequestStatus.Reopened, "status");
    }

    public static void BookingRequestRejectsMovesFromArchived()
    {
        var item = BookingRequest.NewBookingRequest("code-336", "title-371");
        item.Status = BookingRequestStatus.Archived;
        Check.Fails(() => item.MoveTo(BookingRequestStatus.Open), "move");
    }

    public static void RunAll()
    {
        BookingRequestStartsInOpen();
        BookingRequestMovesToReopened();
        BookingRequestRejectsMovesFromArchived();
    }
}
