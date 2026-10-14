namespace Acme.Workforce.Workplace.Tests.Booking.BookingAudit;

using Acme.Workforce.Workplace.Booking.BookingAudit;

/// <summary>Checks of booking audit code, run by the project's runner.</summary>
public static class ScheduledBookingAuditChecks
{
    public static void ScheduledBookingAuditStartsInPlanned()
    {
        var item = ScheduledBookingAudit.NewScheduledBookingAudit("code-692", "title-203");
        Check.Equal(item.Status, ScheduledBookingAuditStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledBookingAuditMovesToScheduled()
    {
        var item = ScheduledBookingAudit.NewScheduledBookingAudit("code-692", "title-203");
        try
        {
            item.MoveTo(ScheduledBookingAuditStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledBookingAuditStatus.Scheduled, "status");
    }

    public static void ScheduledBookingAuditRejectsMovesFromCancelled()
    {
        var item = ScheduledBookingAudit.NewScheduledBookingAudit("code-692", "title-203");
        item.Status = ScheduledBookingAuditStatus.Cancelled;
        Check.Fails(() => item.MoveTo(ScheduledBookingAuditStatus.Planned), "move");
    }

    public static void RunAll()
    {
        ScheduledBookingAuditStartsInPlanned();
        ScheduledBookingAuditMovesToScheduled();
        ScheduledBookingAuditRejectsMovesFromCancelled();
    }
}
