namespace Acme.Workforce.Workplace.Tests.Booking.Window;

using Acme.Workforce.Workplace.Booking.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class BookingBudgetChecks
{
    public static void BookingBudgetStartsInDraft()
    {
        var item = BookingBudget.NewBookingBudget("code-305", "label-878");
        Check.Equal(item.Status, BookingBudgetStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BookingBudgetMovesToSuspended()
    {
        var item = BookingBudget.NewBookingBudget("code-305", "label-878");
        try
        {
            item.MoveTo(BookingBudgetStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BookingBudgetStatus.Suspended, "status");
    }

    public static void BookingBudgetRejectsMovesFromClosed()
    {
        var item = BookingBudget.NewBookingBudget("code-305", "label-878");
        item.Status = BookingBudgetStatus.Closed;
        Check.Fails(() => item.MoveTo(BookingBudgetStatus.Draft), "move");
    }

    public static void RunAll()
    {
        BookingBudgetStartsInDraft();
        BookingBudgetMovesToSuspended();
        BookingBudgetRejectsMovesFromClosed();
    }
}
