namespace Acme.Workforce.Workplace.Tests.Booking.BookingSegment;

using Acme.Workforce.Workplace.Booking.BookingSegment;

/// <summary>Checks of booking segment code, run by the project's runner.</summary>
public static class ScheduledBookingBaselineChecks
{
    public static void ScheduledBookingBaselineStartsInPlanned()
    {
        var item = ScheduledBookingBaseline.NewScheduledBookingBaseline("code-852", "title-869");
        Check.Equal(item.Status, ScheduledBookingBaselineStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledBookingBaselineMovesToScheduled()
    {
        var item = ScheduledBookingBaseline.NewScheduledBookingBaseline("code-852", "title-869");
        try
        {
            item.MoveTo(ScheduledBookingBaselineStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledBookingBaselineStatus.Scheduled, "status");
    }

    public static void ScheduledBookingBaselineRejectsMovesFromCancelled()
    {
        var item = ScheduledBookingBaseline.NewScheduledBookingBaseline("code-852", "title-869");
        item.Status = ScheduledBookingBaselineStatus.Cancelled;
        Check.Fails(() => item.MoveTo(ScheduledBookingBaselineStatus.Planned), "move");
    }

    public static void RunAll()
    {
        ScheduledBookingBaselineStartsInPlanned();
        ScheduledBookingBaselineMovesToScheduled();
        ScheduledBookingBaselineRejectsMovesFromCancelled();
    }
}
