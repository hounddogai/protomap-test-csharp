namespace Acme.Workforce.Workplace.Tests.Booking.BookingPlan;

using Acme.Workforce.Workplace.Booking.BookingPlan;

/// <summary>Checks of booking plan code, run by the project's runner.</summary>
public static class LenientBookingPlanChecks
{
    public static void LenientBookingPlanStartsInNew()
    {
        var item = LenientBookingPlan.NewLenientBookingPlan("code-644", "name-892");
        Check.Equal(item.Status, LenientBookingPlanStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientBookingPlanMovesToRunning()
    {
        var item = LenientBookingPlan.NewLenientBookingPlan("code-644", "name-892");
        try
        {
            item.MoveTo(LenientBookingPlanStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientBookingPlanStatus.Running, "status");
    }

    public static void LenientBookingPlanRejectsMovesFromFailed()
    {
        var item = LenientBookingPlan.NewLenientBookingPlan("code-644", "name-892");
        item.Status = LenientBookingPlanStatus.Failed;
        Check.Fails(() => item.MoveTo(LenientBookingPlanStatus.New), "move");
    }

    public static void RunAll()
    {
        LenientBookingPlanStartsInNew();
        LenientBookingPlanMovesToRunning();
        LenientBookingPlanRejectsMovesFromFailed();
    }
}
