namespace Acme.Workforce.Workplace.Tests.Booking.BookingSegment;

using Acme.Workforce.Workplace.Booking.BookingSegment;

/// <summary>Checks of booking segment code, run by the project's runner.</summary>
public static class LegacyBookingVarianceChecks
{
    public static void LegacyBookingVarianceStartsInOpen()
    {
        var item = LegacyBookingVariance.NewLegacyBookingVariance("code-157", "name-577");
        Check.Equal(item.Status, LegacyBookingVarianceStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyBookingVarianceMovesToInReview()
    {
        var item = LegacyBookingVariance.NewLegacyBookingVariance("code-157", "name-577");
        try
        {
            item.MoveTo(LegacyBookingVarianceStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyBookingVarianceStatus.InReview, "status");
    }

    public static void LegacyBookingVarianceRejectsMovesFromArchived()
    {
        var item = LegacyBookingVariance.NewLegacyBookingVariance("code-157", "name-577");
        item.Status = LegacyBookingVarianceStatus.Archived;
        Check.Fails(() => item.MoveTo(LegacyBookingVarianceStatus.Open), "move");
    }

    public static void RunAll()
    {
        LegacyBookingVarianceStartsInOpen();
        LegacyBookingVarianceMovesToInReview();
        LegacyBookingVarianceRejectsMovesFromArchived();
    }
}
