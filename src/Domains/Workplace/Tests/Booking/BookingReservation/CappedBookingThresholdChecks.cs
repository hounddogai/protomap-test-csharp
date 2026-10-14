namespace Acme.Workforce.Workplace.Tests.Booking.BookingReservation;

using Acme.Workforce.Workplace.Booking.BookingReservation;

/// <summary>Checks of booking reservation code, run by the project's runner.</summary>
public static class CappedBookingThresholdChecks
{
    public static void CappedBookingThresholdStartsInNew()
    {
        var item = CappedBookingThreshold.NewCappedBookingThreshold("code-643", "name-632");
        Check.Equal(item.Status, CappedBookingThresholdStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedBookingThresholdMovesToQueued()
    {
        var item = CappedBookingThreshold.NewCappedBookingThreshold("code-643", "name-632");
        try
        {
            item.MoveTo(CappedBookingThresholdStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedBookingThresholdStatus.Queued, "status");
    }

    public static void CappedBookingThresholdRejectsMovesFromFailed()
    {
        var item = CappedBookingThreshold.NewCappedBookingThreshold("code-643", "name-632");
        item.Status = CappedBookingThresholdStatus.Failed;
        Check.Fails(() => item.MoveTo(CappedBookingThresholdStatus.New), "move");
    }

    public static void RunAll()
    {
        CappedBookingThresholdStartsInNew();
        CappedBookingThresholdMovesToQueued();
        CappedBookingThresholdRejectsMovesFromFailed();
    }
}
