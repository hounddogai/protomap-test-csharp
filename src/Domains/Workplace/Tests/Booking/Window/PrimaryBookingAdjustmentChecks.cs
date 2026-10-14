namespace Acme.Workforce.Workplace.Tests.Booking.Window;

using Acme.Workforce.Workplace.Booking.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class PrimaryBookingAdjustmentChecks
{
    public static void PrimaryBookingAdjustmentStartsInOpen()
    {
        var item = PrimaryBookingAdjustment.NewPrimaryBookingAdjustment("code-118", "name-144");
        Check.Equal(item.Status, PrimaryBookingAdjustmentStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryBookingAdjustmentMovesToInReview()
    {
        var item = PrimaryBookingAdjustment.NewPrimaryBookingAdjustment("code-118", "name-144");
        try
        {
            item.MoveTo(PrimaryBookingAdjustmentStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryBookingAdjustmentStatus.InReview, "status");
    }

    public static void PrimaryBookingAdjustmentRejectsMovesFromArchived()
    {
        var item = PrimaryBookingAdjustment.NewPrimaryBookingAdjustment("code-118", "name-144");
        item.Status = PrimaryBookingAdjustmentStatus.Archived;
        Check.Fails(() => item.MoveTo(PrimaryBookingAdjustmentStatus.Open), "move");
    }

    public static void RunAll()
    {
        PrimaryBookingAdjustmentStartsInOpen();
        PrimaryBookingAdjustmentMovesToInReview();
        PrimaryBookingAdjustmentRejectsMovesFromArchived();
    }
}
