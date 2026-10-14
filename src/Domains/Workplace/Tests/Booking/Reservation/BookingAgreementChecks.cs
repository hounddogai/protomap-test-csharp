namespace Acme.Workforce.Workplace.Tests.Booking.Reservation;

using Acme.Workforce.Workplace.Booking.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class BookingAgreementChecks
{
    public static void BookingAgreementStartsInOpen()
    {
        var item = BookingAgreement.NewBookingAgreement("code-870", "name-657");
        Check.Equal(item.Status, BookingAgreementStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BookingAgreementMovesToReopened()
    {
        var item = BookingAgreement.NewBookingAgreement("code-870", "name-657");
        try
        {
            item.MoveTo(BookingAgreementStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BookingAgreementStatus.Reopened, "status");
    }

    public static void BookingAgreementRejectsMovesFromArchived()
    {
        var item = BookingAgreement.NewBookingAgreement("code-870", "name-657");
        item.Status = BookingAgreementStatus.Archived;
        Check.Fails(() => item.MoveTo(BookingAgreementStatus.Open), "move");
    }

    public static void RunAll()
    {
        BookingAgreementStartsInOpen();
        BookingAgreementMovesToReopened();
        BookingAgreementRejectsMovesFromArchived();
    }
}
