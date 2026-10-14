namespace Acme.Workforce.Workplace.Tests.Booking.Window;

using Acme.Workforce.Workplace.Booking.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class PrimaryBookingWindowChecks
{
    public static void PrimaryBookingWindowStartsInPlanned()
    {
        var item = PrimaryBookingWindow.NewPrimaryBookingWindow("code-997", "name-788");
        Check.Equal(item.Status, PrimaryBookingWindowStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryBookingWindowMovesToScheduled()
    {
        var item = PrimaryBookingWindow.NewPrimaryBookingWindow("code-997", "name-788");
        try
        {
            item.MoveTo(PrimaryBookingWindowStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryBookingWindowStatus.Scheduled, "status");
    }

    public static void PrimaryBookingWindowRejectsMovesFromCancelled()
    {
        var item = PrimaryBookingWindow.NewPrimaryBookingWindow("code-997", "name-788");
        item.Status = PrimaryBookingWindowStatus.Cancelled;
        Check.Fails(() => item.MoveTo(PrimaryBookingWindowStatus.Planned), "move");
    }

    public static void RunAll()
    {
        PrimaryBookingWindowStartsInPlanned();
        PrimaryBookingWindowMovesToScheduled();
        PrimaryBookingWindowRejectsMovesFromCancelled();
    }
}
