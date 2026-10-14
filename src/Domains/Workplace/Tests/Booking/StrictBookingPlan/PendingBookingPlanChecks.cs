namespace Acme.Workforce.Workplace.Tests.Booking.StrictBookingPlan;

using Acme.Workforce.Workplace.Booking.StrictBookingPlan;

/// <summary>Checks of strict booking plan code, run by the project's runner.</summary>
public static class PendingBookingPlanChecks
{
    public static void PendingBookingPlanStartsInDraft()
    {
        var item = PendingBookingPlan.NewPendingBookingPlan("code-998", "label-164");
        Check.Equal(item.Status, PendingBookingPlanStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingBookingPlanMovesToSuspended()
    {
        var item = PendingBookingPlan.NewPendingBookingPlan("code-998", "label-164");
        try
        {
            item.MoveTo(PendingBookingPlanStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingBookingPlanStatus.Suspended, "status");
    }

    public static void PendingBookingPlanRejectsMovesFromClosed()
    {
        var item = PendingBookingPlan.NewPendingBookingPlan("code-998", "label-164");
        item.Status = PendingBookingPlanStatus.Closed;
        Check.Fails(() => item.MoveTo(PendingBookingPlanStatus.Draft), "move");
    }

    public static void RunAll()
    {
        PendingBookingPlanStartsInDraft();
        PendingBookingPlanMovesToSuspended();
        PendingBookingPlanRejectsMovesFromClosed();
    }
}
