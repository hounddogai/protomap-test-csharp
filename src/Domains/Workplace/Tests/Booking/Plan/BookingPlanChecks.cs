namespace Acme.Workforce.Workplace.Tests.Booking.Plan;

using Acme.Workforce.Workplace.Booking.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class BookingPlanChecks
{
    public static void BookingPlanStartsInDraft()
    {
        var item = BookingPlan.NewBookingPlan("code-618", "name-584");
        Check.Equal(item.Status, BookingPlanStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BookingPlanMovesToActive()
    {
        var item = BookingPlan.NewBookingPlan("code-618", "name-584");
        try
        {
            item.MoveTo(BookingPlanStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BookingPlanStatus.Active, "status");
    }

    public static void BookingPlanRejectsMovesFromClosed()
    {
        var item = BookingPlan.NewBookingPlan("code-618", "name-584");
        item.Status = BookingPlanStatus.Closed;
        Check.Fails(() => item.MoveTo(BookingPlanStatus.Draft), "move");
    }

    public static void RunAll()
    {
        BookingPlanStartsInDraft();
        BookingPlanMovesToActive();
        BookingPlanRejectsMovesFromClosed();
    }
}
