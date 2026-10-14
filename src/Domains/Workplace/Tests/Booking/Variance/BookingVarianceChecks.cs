namespace Acme.Workforce.Workplace.Tests.Booking.Variance;

using Acme.Workforce.Workplace.Booking.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class BookingVarianceChecks
{
    public static void BookingVarianceStartsInNew()
    {
        var item = BookingVariance.NewBookingVariance("code-702", "title-721");
        Check.Equal(item.Status, BookingVarianceStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BookingVarianceMovesToQueued()
    {
        var item = BookingVariance.NewBookingVariance("code-702", "title-721");
        try
        {
            item.MoveTo(BookingVarianceStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BookingVarianceStatus.Queued, "status");
    }

    public static void BookingVarianceRejectsMovesFromFailed()
    {
        var item = BookingVariance.NewBookingVariance("code-702", "title-721");
        item.Status = BookingVarianceStatus.Failed;
        Check.Fails(() => item.MoveTo(BookingVarianceStatus.New), "move");
    }

    public static void RunAll()
    {
        BookingVarianceStartsInNew();
        BookingVarianceMovesToQueued();
        BookingVarianceRejectsMovesFromFailed();
    }
}
