namespace Acme.Workforce.Workplace.Tests.Booking.Variance;

using Acme.Workforce.Workplace.Booking.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class BookingAllocationChecks
{
    public static void BookingAllocationStartsInIdle()
    {
        var item = BookingAllocation.NewBookingAllocation("code-536", "label-779");
        Check.Equal(item.Status, BookingAllocationStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BookingAllocationMovesToWarming()
    {
        var item = BookingAllocation.NewBookingAllocation("code-536", "label-779");
        try
        {
            item.MoveTo(BookingAllocationStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BookingAllocationStatus.Warming, "status");
    }

    public static void BookingAllocationRejectsMovesFromStopped()
    {
        var item = BookingAllocation.NewBookingAllocation("code-536", "label-779");
        item.Status = BookingAllocationStatus.Stopped;
        Check.Fails(() => item.MoveTo(BookingAllocationStatus.Idle), "move");
    }

    public static void RunAll()
    {
        BookingAllocationStartsInIdle();
        BookingAllocationMovesToWarming();
        BookingAllocationRejectsMovesFromStopped();
    }
}
