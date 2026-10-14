namespace Acme.Workforce.Workplace.Tests.Booking.Review;

using Acme.Workforce.Workplace.Booking.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class BookingReviewChecks
{
    public static void BookingReviewStartsInDraft()
    {
        var item = BookingReview.NewBookingReview("code-75", "title-281");
        Check.Equal(item.Status, BookingReviewStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BookingReviewMovesToSuspended()
    {
        var item = BookingReview.NewBookingReview("code-75", "title-281");
        try
        {
            item.MoveTo(BookingReviewStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BookingReviewStatus.Suspended, "status");
    }

    public static void BookingReviewRejectsMovesFromClosed()
    {
        var item = BookingReview.NewBookingReview("code-75", "title-281");
        item.Status = BookingReviewStatus.Closed;
        Check.Fails(() => item.MoveTo(BookingReviewStatus.Draft), "move");
    }

    public static void RunAll()
    {
        BookingReviewStartsInDraft();
        BookingReviewMovesToSuspended();
        BookingReviewRejectsMovesFromClosed();
    }
}
