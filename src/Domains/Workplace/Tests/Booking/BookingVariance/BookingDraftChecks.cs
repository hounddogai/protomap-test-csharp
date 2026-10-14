namespace Acme.Workforce.Workplace.Tests.Booking.BookingVariance;

using Acme.Workforce.Workplace.Booking.BookingVariance;

/// <summary>Checks of booking variance code, run by the project's runner.</summary>
public static class BookingDraftChecks
{
    public static void BookingDraftStartsInCreated()
    {
        var item = BookingDraft.NewBookingDraft("code-80", "title-795");
        Check.Equal(item.Status, BookingDraftStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BookingDraftMovesToValidated()
    {
        var item = BookingDraft.NewBookingDraft("code-80", "title-795");
        try
        {
            item.MoveTo(BookingDraftStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BookingDraftStatus.Validated, "status");
    }

    public static void BookingDraftRejectsMovesFromRetired()
    {
        var item = BookingDraft.NewBookingDraft("code-80", "title-795");
        item.Status = BookingDraftStatus.Retired;
        Check.Fails(() => item.MoveTo(BookingDraftStatus.Created), "move");
    }

    public static void RunAll()
    {
        BookingDraftStartsInCreated();
        BookingDraftMovesToValidated();
        BookingDraftRejectsMovesFromRetired();
    }
}
