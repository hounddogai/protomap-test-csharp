namespace Acme.Workforce.Workplace.Tests.Booking.BookingSegment;

using Acme.Workforce.Workplace.Booking.BookingSegment;

/// <summary>Checks of booking segment code, run by the project's runner.</summary>
public static class SeasonalBookingSegmentChecks
{
    public static void SeasonalBookingSegmentStartsInNew()
    {
        var item = SeasonalBookingSegment.NewSeasonalBookingSegment("code-404", "name-512");
        Check.Equal(item.Status, SeasonalBookingSegmentStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalBookingSegmentMovesToQueued()
    {
        var item = SeasonalBookingSegment.NewSeasonalBookingSegment("code-404", "name-512");
        try
        {
            item.MoveTo(SeasonalBookingSegmentStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalBookingSegmentStatus.Queued, "status");
    }

    public static void SeasonalBookingSegmentRejectsMovesFromFailed()
    {
        var item = SeasonalBookingSegment.NewSeasonalBookingSegment("code-404", "name-512");
        item.Status = SeasonalBookingSegmentStatus.Failed;
        Check.Fails(() => item.MoveTo(SeasonalBookingSegmentStatus.New), "move");
    }

    public static void RunAll()
    {
        SeasonalBookingSegmentStartsInNew();
        SeasonalBookingSegmentMovesToQueued();
        SeasonalBookingSegmentRejectsMovesFromFailed();
    }
}
